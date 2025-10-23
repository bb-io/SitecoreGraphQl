using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
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
    public async Task<SearchContentResponse> SearchContent([ActionParameter] SearchContentRequest searchContentRequest)
    {
        var allItems = new List<ContentResponse>();
        var pageSize = 25;
        var pageIndex = 0;
        var totalCount = 0;
        
        do
        {
            var apiRequest = new Request(CredentialsProviders)
                .AddJsonBody(new
                {
                    query = GraphQlQueries.SearchItemsQuery(),
                    variables = new
                    {
                        language = searchContentRequest.Language,
                        pageIndex,
                        pageSize
                    }
                });

            var searchResult = await Client.ExecuteGraphQlWithErrorHandling<SearchItemsWrapperDto>(apiRequest);
            totalCount = searchResult.Search.TotalCount;
            
            foreach (var item in searchResult.Search.Results)
            {
                if (item.InnerItem != null)
                {
                    allItems.Add(item.InnerItem);
                }
                else
                {
                    allItems.Add(new ContentResponse
                    {
                        Id = item.ItemId,
                        Name = item.Name,
                        Path = item.Path,
                        Version = item.Version,
                        WorkflowInfo = new ItemWorkflowResponse(),
                        Fields = new FieldsResponse()
                    });
                }
            }
            
            pageIndex++;
            
        } while (allItems.Count < totalCount);

        if (searchContentRequest.Language != null)
        {
            allItems = allItems
                .Where(item => item.Language.Name.Equals(searchContentRequest.Language, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
    public async Task<FileResponse> DownloadItemContent([ActionParameter] ContentRequest contentRequest)
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
        
        var htmlString = FieldsToHtmlConverter.ConvertToHtml(
            new Models.Records.ContentMetadata(
                contentRequest.ContentId,
                contentRequest.Version,
                contentRequest.Language),
            item.Content.Fields.Nodes);
        
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

        var fields = HtmlToFieldsConverter.ConvertToFields(htmlString);
        var metadata = HtmlToFieldsConverter.ExtractMetadata(htmlString);
        metadata = metadata with
        {
            ContentId = uploadContentRequest.ContentId ?? metadata.ContentId,
            TargetLanguage = uploadContentRequest.Locale ?? throw new PluginMisconfigurationException("Locale must be provided in the upload request")
        };
        
        var targetContent = await GetContent(new ContentRequest
        {
            ContentId = metadata.ContentId,
            Language = metadata.TargetLanguage
        });

        if (targetContent.Version == 0)
        {
            var createItemVersionMutation = GraphQlMutations.AddItemVersionMutation(metadata.ContentId, metadata.TargetLanguage);
            var createVersionRequest = new Request(CredentialsProviders)
                .AddJsonBody(new
                {
                    query = createItemVersionMutation
                });
            
            await Client.ExecuteGraphQlWithErrorHandling<AddItemVersionWrapperDto>(createVersionRequest);
        }
        
        var mutation = GraphQlMutations.UpdateItemMutation(metadata, fields);
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = mutation
            });

        await Client.ExecuteGraphQlWithErrorHandling<UpdateItemWrapperDto>(apiRequest);
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
}