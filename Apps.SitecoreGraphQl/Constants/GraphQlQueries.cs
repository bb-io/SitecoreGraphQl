using Apps.SitecoreGraphQl.Models.Requests;

namespace Apps.SitecoreGraphQl.Constants;

public static class GraphQlQueries
{
    private const string GetLanguages = "query { languages { nodes { iso name displayName } } }";
    
    private const string GetItemById = @"query { item( where: { database: ""master"", itemId: ""{ITEM_ID}"", language: {LANGUAGE}, version: {VERSION} }) { itemId name path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: true, excludeStandardFields: true) { nodes {  name value } } } }";
    
    private const string GetWorkflowCommands = @"query GetWorkflowCommands( $workflowId: String!, $stateId: String! ) { workflow(where: { workflowId: $workflowId }) { workflowId displayName commands(query: { stateId: $stateId }) { nodes { commandId displayName } pageInfo { hasNextPage endCursor } } } }";
    
    public static string GetLanguagesQuery()
    {
        return GetLanguages;
    }
    
    public static string GetItemByIdQuery(ContentRequest contentRequest)
    {
        var itemId = contentRequest.GetContentId();
        var finalQuery = GetItemById
            .Replace("{ITEM_ID}", itemId)
            .Replace("{LANGUAGE}", contentRequest.Language == null ? "null" : $"\"{contentRequest.Language}\"")
            .Replace("{VERSION}", contentRequest.Version.HasValue ? $"{contentRequest.Version.Value}" : "null");
        
        return finalQuery;
    }
    
    public static string GetWorkflowCommandsQuery()
    {
        return GetWorkflowCommands;
    }
}