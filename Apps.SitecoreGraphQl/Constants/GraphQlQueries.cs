namespace Apps.SitecoreGraphQl.Constants;

public static class GraphQlQueries
{
    private const string GetLanguages = "query { languages { nodes { name } } }";
    
    private const string GetItemById = @"query { item( where: { database: ""master"", itemId: ""{ITEM_ID}"" }) { itemId name path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: true, excludeStandardFields: true) { nodes {  name value } } } }";
    
    private const string GetWorkflowCommands = @"query GetWorkflowCommands( $workflowId: String!, $stateId: String! ) { workflow(where: { workflowId: $workflowId }) { workflowId displayName commands(query: { stateId: $stateId }) { nodes { commandId displayName } pageInfo { hasNextPage endCursor } } } }";
    
    public static string GetLanguagesQuery()
    {
        return GetLanguages;
    }
    
    public static string GetItemByIdQuery(string itemId)
    {
        return GetItemById.Replace("{ITEM_ID}", itemId);
    }
    
    public static string GetWorkflowCommandsQuery()
    {
        return GetWorkflowCommands;
    }
}