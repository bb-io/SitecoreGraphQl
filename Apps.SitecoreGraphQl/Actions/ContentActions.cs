using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Utils;
using Apps.SitecoreGraphQl.Utils.Converters;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
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
        
        var criteria = await BuildScopeCriteriaAsync(searchContentRequest, dateFilters);
        var fieldFilters = GetFieldFilters(searchContentRequest.FieldNames, searchContentRequest.FieldValues);

        List<ContentResponse> allItems;
        if (searchContentRequest.UseExactFieldValueFiltering == true && fieldFilters.Count > 0)
        {
            var exactSearchParams = new SearchContentParams(
                searchContentRequest.Language,
                criteria.Count > 0 ? criteria : null);
            allItems = await Client.SearchContentAsync(exactSearchParams, CredentialsProviders);
            allItems = FilterItemsByExactFieldValues(allItems, fieldFilters).ToList();
        }
        else
        {
            var subCriteria = BuildFieldSubCriteria(criteria, fieldFilters);
            var searchParams = new SearchContentParams(
                searchContentRequest.Language,
                criteria.Count > 0 ? criteria : null,
                subCriteria.Count > 0 ? subCriteria : null);
            allItems = await Client.SearchContentAsync(searchParams, CredentialsProviders);
        }
        
        return new SearchContentResponse
        {
            Items = allItems
        };
    }
    
    [Action("Get content", Description = "Get an content (item) by its ID")]
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
        if (string.IsNullOrEmpty(contentRequest.Language))
            throw new PluginMisconfigurationException("Language must be specified to download content.");

        var rootItem = await FetchItemForDownloadAsync(contentRequest);

        var entities = new List<ContentWithFieldsEntity>
        {
            ToContentEntity(rootItem, filteringOptions, isRoot: true)
        };

        if (downloadContentRequest.IncludeChildItems == true)
        {
            var childItems = await FetchChildItemsAsync(contentRequest.Language, rootItem.Id, downloadContentRequest);
            entities.AddRange(childItems.Select(item => ToContentEntity(item, filteringOptions, isRoot: false)));
        }

        var rootMetadata = new ContentMetadata(
            contentRequest.ContentId,
            contentRequest.Version,
            contentRequest.Language,
            RootContentId: contentRequest.ContentId);

        return await BuildHtmlFileAsync(rootMetadata, entities, rootItem.Name);
    }

    [Action("Upload content", Description = "Upload translated content back to Sitecore")]
    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    public async Task UploadItemContent([ActionParameter] UploadContentRequest uploadContentRequest)
    {
        var targetLanguage = uploadContentRequest.Locale
                             ?? throw new PluginMisconfigurationException(
                                 "Locale must be provided in the upload request");

        var htmlString = await ReadHtmlFromFileAsync(uploadContentRequest.Content);
        var contentEntities = HtmlToFieldsConverter.ConvertToContentEntities(htmlString);

        foreach (var entity in contentEntities)
        {
            var contentId = ResolveContentId(entity, uploadContentRequest.ContentId);
            var targetVersion = await EnsureTargetVersionAsync(contentId, targetLanguage);
            await UpdateItemFieldsAsync(contentId, targetVersion, entity, targetLanguage);
        }
    }

    private async Task<ContentResponse> FetchItemForDownloadAsync(ContentRequest request)
    {
        var query = GraphQlQueries.GetItemByIdQuery(request, ownFields: true);
        var apiRequest = new Request(CredentialsProviders).AddJsonBody(new { query });
        var result = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(apiRequest);

        if (result.Content == null)
            throw new PluginApplicationException(
                $"Item with ID {request.ContentId} was not found. Please verify the ID and try again.");

        return result.Content;
    }

    private static ContentWithFieldsEntity ToContentEntity(ContentResponse item, FilteringOptions filteringOptions, bool isRoot)
    {
        var fields = filteringOptions.ApplyFilteringOptions(item.Fields.Nodes);
        return new ContentWithFieldsEntity(item.Id, item.Version, item.Language.Name, fields, IsRootContent: isRoot);
    }

    private async Task<List<ContentResponse>> FetchChildItemsAsync(
        string language, string rootItemId, DownloadContentRequest request)
    {
        var fieldFilters = GetFieldFilters(request.FieldNames, request.FieldValues);

        var pathCriteria = new List<CriteriaDto>
        {
            new() { Field = "_path", CriteriaType = "SEARCH", Operator = "MUST", Value = rootItemId }
        };
        var fieldSubCriteria = fieldFilters
            .Select(f => new CriteriaDto { Field = f.Key, CriteriaType = "WILDCARD", Operator = "MUST", Value = f.Value })
            .ToList();

        var searchParams = new SearchContentParams(
            language,
            pathCriteria,
            fieldSubCriteria.Count > 0 ? fieldSubCriteria : null,
            IncludeOnlyOwnFields: true,
            ExcludeStandardFields: true);

        var items = await Client.SearchContentAsync(searchParams, CredentialsProviders);

        return fieldFilters.Count > 0
            ? FilterItemsByFieldPairs(items, fieldFilters).ToList()
            : items;
    }

    private async Task<FileResponse> BuildHtmlFileAsync(ContentMetadata metadata, List<ContentWithFieldsEntity> entities, string itemName)
    {
        var html = FieldsToHtmlConverter.ConvertToHtml(metadata, entities);
        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        using var stream = new MemoryStream(bytes);
        var fileRef = await fileManagementClient.UploadAsync(stream, "text/html", $"{itemName}.html");
        return new() { Content = fileRef };
    }

    private async Task<string> ReadHtmlFromFileAsync(Blackbird.Applications.Sdk.Common.Files.FileReference fileReference)
    {
        var fileStream = await fileManagementClient.DownloadAsync(fileReference);
        var bytes = await fileStream.GetByteData();
        var htmlString = System.Text.Encoding.UTF8.GetString(bytes);

        if (!Xliff2Serializer.IsXliff2(htmlString))
            return htmlString;

        var converted = Transformation.Parse(htmlString, fileReference.Name).Target().Serialize();
        return converted ?? throw new PluginMisconfigurationException("XLIFF did not contain any files");
    }

    private static string ResolveContentId(ContentWithFieldsEntity entity, string? overrideContentId)
    {
        return entity.IsRootContent && !string.IsNullOrEmpty(overrideContentId)
            ? overrideContentId
            : entity.ContentId;
    }

    private async Task<int> EnsureTargetVersionAsync(string contentId, string targetLanguage)
    {
        var targetContent = await GetContent(new ContentRequest
        {
            ContentId = contentId,
            Language = targetLanguage
        });

        if (targetContent.Version > 0)
            return targetContent.Version;

        var addedVersion = await Client.ExecuteGraphQlWithErrorHandling<AddItemVersionWrapperDto>(
            new Request(CredentialsProviders).AddJsonBody(new
            {
                query = GraphQlMutations.AddItemVersionMutation(contentId, targetLanguage)
            }));

        return addedVersion.AddItemVersion.Item.Version;
    }

    private async Task UpdateItemFieldsAsync(string contentId, int targetVersion, ContentWithFieldsEntity entity, string targetLanguage)
    {
        var metadata = new ContentMetadata(contentId, targetVersion, entity.SourceLanguage, TargetLanguage: targetLanguage);
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new { query = GraphQlMutations.UpdateItemMutation(metadata, entity.Fields) });

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

    private static IEnumerable<ContentResponse> FilterItemsByFieldPairs(
        IEnumerable<ContentResponse> items, IReadOnlyList<KeyValuePair<string, string>> fieldFilters)
    {
        return items.Where(item =>
            fieldFilters.All(f => SearchFieldValueMatcher.Matches(item.Fields.Nodes, f.Key, f.Value)));
    }

    private async Task<List<CriteriaDto>> BuildScopeCriteriaAsync(SearchContentRequest searchContentRequest, DateFilters dateFilters)
    {
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

        return criteria;
    }

    private static List<KeyValuePair<string, string>> GetFieldFilters(IEnumerable<string>? fieldNames, IEnumerable<string>? fieldValues)
    {
        if (fieldNames == null || fieldValues == null)
        {
            return [];
        }

        var fieldNamesList = fieldNames.ToList();
        var fieldValuesList = fieldValues.ToList();
        if (fieldNamesList.Count != fieldValuesList.Count)
        {
            throw new PluginMisconfigurationException("Field names and field values counts do not match.");
        }

        var fieldFilters = new List<KeyValuePair<string, string>>();
        for (var index = 0; index < fieldNamesList.Count; index++)
        {
            fieldFilters.Add(new KeyValuePair<string, string>(fieldNamesList[index], fieldValuesList[index]));
        }

        return fieldFilters;
    }

    private static List<CriteriaDto> BuildFieldSubCriteria(List<CriteriaDto> criteria, IReadOnlyList<KeyValuePair<string, string>> fieldFilters)
    {
        var subCriteria = new List<CriteriaDto>();
        if (fieldFilters.Count == 0)
        {
            return subCriteria;
        }

        var isMainCriteriaEmpty = criteria.Count == 0;
        foreach (var fieldFilter in fieldFilters)
        {
            var targetCriteriaCollection = isMainCriteriaEmpty ? criteria : subCriteria;
            targetCriteriaCollection.Add(new CriteriaDto
            {
                Field = fieldFilter.Key,
                CriteriaType = "WILDCARD",
                Operator = "MUST",
                Value = fieldFilter.Value
            });

            isMainCriteriaEmpty = false;
        }

        return subCriteria;
    }

    private static IEnumerable<ContentResponse> FilterItemsByExactFieldValues(IEnumerable<ContentResponse> items, IReadOnlyList<KeyValuePair<string, string>> fieldFilters)
    {
        return items.Where(item => fieldFilters.All(fieldFilter =>
            SearchFieldValueMatcher.MatchesExactFieldValue(item.Fields.Nodes, fieldFilter.Key, fieldFilter.Value)));
    }
}
