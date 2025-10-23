using Apps.SitecoreGraphQl.Actions;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.SitecoreGraphQl.Handlers;

public class ContentDataSource(InvocationContext invocationContext) : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        var contentActions = new ContentActions(invocationContext, null!);
        var contentResponse = await contentActions.SearchContent(new());
        return contentResponse.Items
            .Where(x => string.IsNullOrEmpty(context.SearchString) || x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(content => new DataSourceItem(content.Id, content.Name));
    }
}