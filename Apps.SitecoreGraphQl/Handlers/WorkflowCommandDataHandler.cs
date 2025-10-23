using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.SitecoreGraphQl.Handlers;

public class WorkflowCommandDataHandler(InvocationContext invocationContext, [ActionParameter] ContentRequest contentRequest) 
    : Invocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(contentRequest.ContentId))
        {
            throw new ArgumentException("Please provide first an Content ID first to retrieve workflow commands.");
        }
        
        var item = await GetItemAsync(contentRequest);
        if (item.WorkflowInfo?.Workflow?.WorkflowId == null || item.WorkflowInfo?.WorkflowState?.WorkflowStateId == null)
        {
            return new List<DataSourceItem>();
        }

        var commands = await GetWorkflowCommandsAsync(item.WorkflowInfo.Workflow.WorkflowId, item.WorkflowInfo.WorkflowState.WorkflowStateId);
        commands = commands.Where(cmd => string.IsNullOrEmpty(context.SearchString) 
                                         || cmd.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).ToList();
        return commands.Select(cmd => new DataSourceItem(cmd.CommandId, cmd.DisplayName));
    }

    private async Task<ContentResponse> GetItemAsync(ContentRequest contentRequest)
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
    
    private async Task<List<WorkflowCommandDto>> GetWorkflowCommandsAsync(string workflowId, string stateId)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlQueries.GetWorkflowCommandsQuery(),
                variables = new
                {
                    workflowId,
                    stateId
                }
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<WorkflowCommandsWrapperDto>(apiRequest);
        return result.Workflow?.Commands?.Nodes ?? new List<WorkflowCommandDto>();
    }
}