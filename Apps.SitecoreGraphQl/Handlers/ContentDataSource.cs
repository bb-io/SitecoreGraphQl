using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SitecoreGraphQl.Handlers;

public class ContentDataSource(InvocationContext invocationContext, [ActionParameter] ContentRequest contentRequest) 
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var criteria = new List<CriteriaDto>();
        var sorts = new List<SortDto>();
        
        if (!string.IsNullOrEmpty(context.SearchString))
        {
            criteria.Add(new CriteriaDto
            {
                Field = "name",
                CriteriaType = "CONTAINS",
                Operator = "MUST",
                Value = context.SearchString
            });
            
            sorts.Add(new SortDto
            {
                Field = "name",
                Direction = "ASCENDING"
            });
        }
        
        var searchParams = new SearchContentParams(
            contentRequest.Language, 
            criteria.Count > 0 ? criteria : null, 
            AutoPagination: false,
            Sort: sorts
        );
        
        var contentResponse = await Client.SearchContentAsync(searchParams, CredentialsProviders);
        return contentResponse.Select(content => new DataSourceItem(content.Id, content.Name));
    }
}