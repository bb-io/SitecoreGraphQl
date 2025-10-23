using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Events.Models;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.SitecoreGraphQl.Events;

[PollingEventList("Content")]
public class ContentPollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [PollingEvent("On content created or updated", Description = "Polling event that periodically checks for new new or updated content. If the new or updated content is found, it will be returned as a list of content items.")]
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdatedMultiple)]
    public async Task<PollingEventResponse<DateMemory, SearchContentResponse>> OnContentCreatedOrUpdatedAsync(PollingEventRequest<DateMemory> request,
        [PollingEventParameter] SearchContentRequest searchContentRequest)
    {
        return await ProcessPollingRequest(request, searchContentRequest);
    }

    private async Task<PollingEventResponse<DateMemory, SearchContentResponse>> ProcessPollingRequest(
        PollingEventRequest<DateMemory> request,
        SearchContentRequest searchContentRequest)
    {
        if (request.Memory == null)
        {
            return new()
            {
                Result = null,
                FlyBird = false,
                Memory = new DateMemory
                {
                    LastPollingTime = DateTime.UtcNow
                }
            };
        }

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
            
            var results = searchResult.Search.Results
                .Where(x => x.CreatedAt >= request.Memory.LastPollingTime || x.UpdatedAt >= request.Memory.LastPollingTime)
                .ToList();
            
            foreach (var item in results)
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
        
        return new PollingEventResponse<DateMemory, SearchContentResponse>
        {
            Result = new SearchContentResponse
            {
                Items = allItems
            },
            FlyBird = allItems.Any(),
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow
            }
        };
    }
}