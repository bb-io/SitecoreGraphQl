using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Utils.Converters;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;
using RestSharp;

namespace Apps.SitecoreGraphQl.Actions;

[ActionList("Content (items)")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) 
    : Invocable(invocationContext)
{
    [Action("Search content", Description = "Retrieve a list of content (items)")]
    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest searchContentRequest,
        [ActionParameter] DateFilters dateFilters)
    {
        if (string.IsNullOrEmpty(searchContentRequest.Language))
        {
            throw new PluginMisconfigurationException("Language must be specified to search content.");
        }
        
        var criteria = new List<CriteriaDto>();
        
        if (!string.IsNullOrEmpty(searchContentRequest.RootPath))
        {
            var pathRequest = new Request(CredentialsProviders)
                .AddJsonBody(new
                {
                    query = GraphQlQueries.GetItemByPathQuery(searchContentRequest.RootPath)
                });

            var pathResult = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(pathRequest);
            if (pathResult.Content == null)
            {
                throw new PluginApplicationException(
                    $"Item with path '{searchContentRequest.RootPath}' was not found. Please provide a correct item path.");
            }
            
            criteria.Add(new CriteriaDto
            {
                Field = "_path",
                CriteriaType = "SEARCH",
                Operator = "MUST",
                Value = pathResult.Content.Id
            });
        }
        
        if (dateFilters.CreatedAfter.HasValue || dateFilters.CreatedBefore.HasValue)
        {
            var createdRange = BuildDateRange(dateFilters.CreatedAfter, dateFilters.CreatedBefore);
            if (!string.IsNullOrEmpty(createdRange))
            {
                criteria.Add(new CriteriaDto
                {
                    Field = "__smallcreateddate",
                    CriteriaType = "RANGE",
                    Operator = "MUST",
                    Value = createdRange
                });
            }
        }
        
        if (dateFilters.UpdatedAfter.HasValue || dateFilters.UpdatedBefore.HasValue)
        {
            var updatedRange = BuildDateRange(dateFilters.UpdatedAfter, dateFilters.UpdatedBefore);
            if (!string.IsNullOrEmpty(updatedRange))
            {
                criteria.Add(new CriteriaDto
                {
                    Field = "__smallupdateddate",
                    CriteriaType = "RANGE",
                    Operator = "MUST",
                    Value = updatedRange
                });
            }
        }
        
        var subCriteria = new List<CriteriaDto>();
        if(searchContentRequest.FieldNames != null && searchContentRequest.FieldValues != null)
        {
            var fieldNames = searchContentRequest.FieldNames.ToList();
            var fieldValues = searchContentRequest.FieldValues.ToList();
            if (fieldNames.Count != fieldValues.Count)
            {
                throw new PluginMisconfigurationException("Field names and field values counts do not match.");
            }

            bool isMainCriteriaEmpty = criteria.Count == 0;
            for (int i = 0; i < fieldNames.Count; i++)
            {
                if (isMainCriteriaEmpty)
                {
                    // If user doesn't provide any main criteria, we add the first field criteria to the main criteria list
                    criteria.Add(new CriteriaDto
                    {
                        Field = fieldNames[i],
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = fieldValues[i]
                    });
                }
                else
                {
                    subCriteria.Add(new CriteriaDto
                    {
                        Field = fieldNames[i],
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = fieldValues[i]
                    });
                }
            }
        }
        
        var searchParams = new SearchContentParams(searchContentRequest.Language, criteria.Count > 0 ? criteria : null, subCriteria.Count > 0 ? subCriteria : null);
        var allItems = await Client.SearchContentAsync(searchParams, CredentialsProviders);
        
        if(searchContentRequest.FieldNames != null && searchContentRequest.FieldValues != null)
        {
            // Sitecore filters is buggy, so sometimes it can return items that do not properly match to filters
            var fieldNames = searchContentRequest.FieldNames.ToList();
            var fieldValues = searchContentRequest.FieldValues.ToList();
            allItems = allItems.Where(item =>
            {
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    var field = item.Fields.Nodes.FirstOrDefault(f => f.Name.Equals(fieldNames[i], StringComparison.OrdinalIgnoreCase));
                    if (field == null || field.Value == null || !field.Value.ToString().Equals(fieldValues[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }).ToList();
        }
        
        return new SearchContentResponse
        {
            Items = allItems
        };
    }
    
    [Action("Get content information", Description = "Get an content (item) by its ID")]
    public async Task<ContentResponse> GetContent([ActionParameter] ContentRequest contentRequest)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlQueries.GetItemByIdQuery(contentRequest)
            });

        var item = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(apiRequest);
        if (item.Content == null)
        {
            throw new PluginApplicationException(
                $"Item with ID {contentRequest.ContentId} was not found. Please verify the ID and try again.");
        }

        return item.Content;
    }
    
    [Action("Download content", Description = "Download the content of a content (item) by its ID")]
    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    public async Task<FileResponse> DownloadItemContent([ActionParameter] ContentRequest contentRequest,
        [ActionParameter] DownloadContentRequest downloadContentRequest,
        [ActionParameter] FilteringOptions filteringOptions)
    {
        if(string.IsNullOrEmpty(contentRequest.Language))
        {
            throw new PluginMisconfigurationException("Language must be specified to download content.");
        }

        var query = GraphQlQueries.GetItemByIdQuery(contentRequest, filteringOptions.IncludeOnlyOwnFields ?? false, filteringOptions.ExcludeStandardFields ?? true);
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query
            });

        var item = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(apiRequest);
        if (item.Content == null)
        {
            throw new PluginApplicationException(
                $"Item with ID {contentRequest.ContentId} was not found. Please verify the ID and try again.");
        }
        
        var fields = filteringOptions.ApplyFilteringOptions(item.Content.Fields.Nodes);
        var rootContentMetadata = new ContentMetadata(
            contentRequest.ContentId, 
            contentRequest.Version, 
            contentRequest.Language, 
            RootContentId: contentRequest.ContentId);
        var fieldsEntities = new List<ContentWithFieldsEntity>
        {
            new(item.Content.Id, item.Content.Version, item.Content.Language.Name, fields, IsRootContent: true)
        };

        if (downloadContentRequest.IncludeChildItems == true)
        {
            var criteria = new List<CriteriaDto>
            {
                new()
                {
                    Field = "_path",
                    CriteriaType = "SEARCH",
                    Operator = "MUST",
                    Value = contentRequest.ContentId
                }
            };
            
            var subCriteria = new List<CriteriaDto>();
            if (downloadContentRequest.FieldNames != null && downloadContentRequest.FieldValues != null)
            {
                var fieldNames = downloadContentRequest.FieldNames.ToList();
                var fieldValues = downloadContentRequest.FieldValues.ToList();
                if (fieldNames.Count != fieldValues.Count)
                {
                    throw new PluginMisconfigurationException("Field names and field values counts do not match.");
                }

                for (int i = 0; i < fieldNames.Count; i++)
                {
                    subCriteria.Add(new CriteriaDto
                    {
                        Field = fieldNames[i],
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = fieldValues[i]
                    });
                }
            }
            
            var searchParams = new SearchContentParams(
                contentRequest.Language,
                criteria,
                subCriteria.Count > 0 ? subCriteria : null,
                IncludeOnlyOwnFields: filteringOptions.IncludeOnlyOwnFields ?? false,
                ExcludeStandardFields: filteringOptions.ExcludeStandardFields ?? true);
            
            var childItems = await Client.SearchContentAsync(searchParams, CredentialsProviders);
            
            // Apply in-memory filtering to ensure correct results (Sitecore filters can be buggy)
            if (downloadContentRequest.FieldNames != null && downloadContentRequest.FieldValues != null)
            {
                var fieldNames = downloadContentRequest.FieldNames.ToList();
                var fieldValues = downloadContentRequest.FieldValues.ToList();
                childItems = childItems.Where(childItem =>
                {
                    for (int i = 0; i < fieldNames.Count; i++)
                    {
                        var field = childItem.Fields.Nodes.FirstOrDefault(f => f.Name.Equals(fieldNames[i], StringComparison.OrdinalIgnoreCase));
                        if (field == null || field.Value == null || !field.Value.ToString().Contains(fieldValues[i], StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToList();
            }
            
            foreach (var childItem in childItems)
            {
                var childFields = filteringOptions.ApplyFilteringOptions(childItem.Fields.Nodes);
                fieldsEntities.Add(new ContentWithFieldsEntity(
                    childItem.Id,
                    childItem.Version,
                    childItem.Language.Name,
                    childFields,
                    IsRootContent: false));
            }
        }
        
        var htmlString = FieldsToHtmlConverter.ConvertToHtml(rootContentMetadata, fieldsEntities);
        
        var bytes = System.Text.Encoding.UTF8.GetBytes(htmlString);
        var memoryStream = new MemoryStream(bytes);
        memoryStream.Position = 0;
        
        var fileReference = await fileManagementClient.UploadAsync(memoryStream, "text/html", $"{item.Content.Name}.html");
        return new()
        {
            Content = fileReference
        };
    }

    [Action("Upload content", Description = "Upload translated content back to Sitecore")]
    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    public async Task UploadItemContent([ActionParameter] UploadContentRequest uploadContentRequest)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadContentRequest.Content);
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var htmlString = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        if (Xliff2Serializer.IsXliff2(htmlString))
        {
            htmlString = Transformation.Parse(htmlString, uploadContentRequest.Content.Name).Target().Serialize();
            if (htmlString == null)
            {
                throw new PluginMisconfigurationException("XLIFF did not contain any files");
            }
        }

        var contentEntities = HtmlToFieldsConverter.ConvertToContentEntities(htmlString);
        
        var targetLanguage = uploadContentRequest.Locale 
            ?? throw new PluginMisconfigurationException("Locale must be provided in the upload request");
        foreach (var entity in contentEntities)
        {
            var contentId = entity.ContentId;
            if (entity.IsRootContent && !string.IsNullOrEmpty(uploadContentRequest.ContentId))
            {
                contentId = uploadContentRequest.ContentId;
            }
            
            var targetContent = await GetContent(new ContentRequest
            {
                ContentId = contentId,
                Language = targetLanguage
            });

            if (targetContent.Version == 0)
            {
                var createItemVersionMutation = GraphQlMutations.AddItemVersionMutation(contentId, targetLanguage);
                var createVersionRequest = new Request(CredentialsProviders)
                    .AddJsonBody(new
                    {
                        query = createItemVersionMutation
                    });
                
                await Client.ExecuteGraphQlWithErrorHandling<AddItemVersionWrapperDto>(createVersionRequest);
            }
            
            var entityMetadata = new ContentMetadata(
                contentId,
                entity.Version,
                entity.SourceLanguage,
                TargetLanguage: targetLanguage
            );
            
            var mutation = GraphQlMutations.UpdateItemMutation(entityMetadata, entity.Fields);
            var apiRequest = new Request(CredentialsProviders)
                .AddJsonBody(new
                {
                    query = mutation
                });

            await Client.ExecuteGraphQlWithErrorHandling<UpdateItemWrapperDto>(apiRequest);
        }
    }
    
    
    [Action("Delete content", Description = "Delete a content (item) by its ID")]
    public async Task DeleteContent([ActionParameter] ContentRequest contentRequest)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlMutations.DeleteItemMutation(contentRequest.GetContentId())
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<DeleteItemWrapperDto>(apiRequest);
        if (!result.DeleteContent.Successful)
        {
            throw new PluginApplicationException(
                $"Failed to delete item with ID {contentRequest.ContentId}. Please verify the ID and try again.");
        }
    }
    
    private static string? BuildDateRange(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "*";
        var to = toDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "NOW";
        if (from == "*" && to == "NOW")
        {
            return null;
        }
        
        return $"[{from} TO {to}]";
    }
}