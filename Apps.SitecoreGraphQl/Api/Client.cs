using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Responses;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.SitecoreGraphQl.Api;

public class Client(List<AuthenticationCredentialsProvider> creds) : BlackBirdRestClient(new()
{
    BaseUrl = creds.GetBaseUrl(),
    ThrowOnAnyError = false
}) 
{
    public async Task<T> ExecuteGraphQlWithErrorHandling<T>(RestRequest request)
    {
        var response = await ExecuteWithErrorHandling(request);
        var graphQlResponse = JsonConvert.DeserializeObject<GraphQlResponseDto<T>>(response.Content!);
        return graphQlResponse!.Data!;
    }
    
    public async Task<List<ContentResponse>> SearchContentAsync(SearchContentParams searchParams, IEnumerable<AuthenticationCredentialsProvider> credentialsProviders)
    {
        var allItems = new List<ContentResponse>();
        var pageSize = 100;
        var pageIndex = 0;
        var totalCount = 0;
        
        do
        {
            var apiRequest = new Request(credentialsProviders);
            
            if (searchParams.Criteria != null && searchParams.Criteria.Count > 0)
            {
                var query = GraphQlQueries.SearchItemsWithCriteriasQuery(
                    searchParams.Criteria, 
                    searchParams.subCriterias, 
                    searchParams.Sort,
                    searchParams.IncludeOnlyOwnFields,
                    searchParams.ExcludeStandardFields);
                apiRequest.AddJsonBody(new
                {
                    query,
                    variables = new
                    {
                        language = searchParams.Language,
                        pageIndex,
                        pageSize
                    }
                });
            }
            else
            {
                apiRequest.AddJsonBody(new
                {
                    query = GraphQlQueries.SearchItemsQuery(),
                    variables = new
                    {
                        language = searchParams.Language,
                        pageIndex,
                        pageSize
                    }
                });
            }

            var searchResult = await ExecuteGraphQlWithErrorHandling<SearchItemsWrapperDto>(apiRequest);
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
            
        } while (searchParams.AutoPagination && allItems.Count < totalCount);

        return allItems;
    }
    
    public override async Task<RestResponse> ExecuteWithErrorHandling(RestRequest request)
    {
        var response = await base.ExecuteWithErrorHandling(request);
        var graphQlResponse = JsonConvert.DeserializeObject<GraphQlResponseDto<object>>(response.Content!);
        if (graphQlResponse != null && graphQlResponse.Errors.Count > 0)
        {
            throw new PluginApplicationException(graphQlResponse.GetErrorMessages());
        }
        
        return response;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        if (string.IsNullOrEmpty(response.Content))
        {
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                return new PluginApplicationException($"Request failed with status code {response.StatusCode}");
            }
            
            return new PluginApplicationException(response.ErrorMessage);
        }

        var error = JsonConvert.DeserializeObject<RestErrorDto>(response.Content!);
        return error == null 
            ? new PluginApplicationException($"Request failed with status code {response.StatusCode}, content: {response.Content}") 
            : new PluginApplicationException($"{error.Error}: {error.ErrorDescription}");
    }
}