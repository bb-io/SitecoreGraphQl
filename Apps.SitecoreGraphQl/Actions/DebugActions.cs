using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.SitecoreGraphQl.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Get schema", Description = "Get the GraphQL schema")]
    public async Task<GetSchemaResponse> GetSchema()
    {
        var query = @"query IntrospectionQuery { __schema { queryType { name } mutationType { name } types { kind name fields { name } } } }";
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query
            });
        
        var item = await Client.ExecuteWithErrorHandling(apiRequest);
        return new GetSchemaResponse
        {
            SchemaJson = item.Content
        };
    }
}