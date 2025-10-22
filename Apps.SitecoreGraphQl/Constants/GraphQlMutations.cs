namespace Apps.SitecoreGraphQl.Constants;

public static class GraphQlMutations
{
    private const string DeleteItem = @"mutation { deleteItem( input: { database: ""master"", itemId: ""{ITEM_ID}"", permanently: false } ) { successful } }";
    
    private const string ExecuteWorkflowCommand = @"mutation { executeWorkflowCommand( input: { commandId: ""{COMMAND_ID}"" item: { itemId: ""{ITEM_ID}"" } } ) { successful nextStateId message completed error } }";
    
    public static string DeleteItemMutation(string itemId)
    {
        return DeleteItem.Replace("{ITEM_ID}", itemId);
    }
    
    public static string ExecuteWorkflowCommandMutation(string commandId, string itemId)
    {
        return ExecuteWorkflowCommand
            .Replace("{COMMAND_ID}", commandId)
            .Replace("{ITEM_ID}", itemId);
    }
}