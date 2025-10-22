using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.SitecoreGraphQl.Actions;

[ActionList("Workflows")]
public class WorkflowActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Update workflow state", Description = "Update the workflow state of an item")]
    public async Task<ExecuteCommandResponse> UpdateWorkflowState([ActionParameter] UpdateWorkflowStateRequest request)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlMutations.ExecuteWorkflowCommandMutation(request.WorkflowCommandId, request.GetItemId())
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<ExecuteCommandWrapperDto>(apiRequest);
        if (!result.ExecuteWorkflowCommand.Successful)
        {
            throw new PluginApplicationException(result.ExecuteWorkflowCommand.Error);
        }
        
        return result.ExecuteWorkflowCommand;
    }
}