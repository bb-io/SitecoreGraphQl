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
    [Action("Search workflows", Description = "Get all workflows with their states")]
    public async Task<SearchWorkflowsResponse> SearchWorkflows()
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlQueries.SearchWorkflowsQuery()
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<WorkflowsWrapperDto>(apiRequest);
        
        return new SearchWorkflowsResponse
        {
            Workflows = result.Workflows.Nodes.Select(w => new WorkflowResponse
            {
                WorkflowId = w.WorkflowId,
                DisplayName = w.DisplayName,
                InitialState = new WorkflowStateResponse
                {
                    WorkflowStateId = w.InitialState.StateId,
                    Name = w.InitialState.DisplayName
                },
                States = w.States.Nodes.Select(s => new WorkflowStateResponse
                {
                    WorkflowStateId = s.StateId,
                    Name = s.DisplayName
                }).ToList()
            }).ToList()
        };
    }
    
    [Action("Update workflow state", Description = "Update the workflow state of an item")]
    public async Task<ExecuteCommandResponse> UpdateWorkflowState([ActionParameter] UpdateWorkflowStateRequest request)
    {
        var mutation = GraphQlMutations.ExecuteWorkflowCommandMutation(request.WorkflowCommandId, request.GetContentId(), request.Language, request.Version);
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = mutation
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<ExecuteCommandWrapperDto>(apiRequest);
        if (!result.ExecuteWorkflowCommand.Successful)
        {
            throw new PluginApplicationException(result.ExecuteWorkflowCommand.Error);
        }
        
        return result.ExecuteWorkflowCommand;
    }
}