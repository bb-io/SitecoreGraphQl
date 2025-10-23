namespace Apps.SitecoreGraphQl.Constants;

using Models.Records;
using Models.Responses;
using System.Text;

public static class GraphQlMutations
{
    private const string DeleteItem = @"mutation { deleteItem( input: { database: ""master"", itemId: ""{ITEM_ID}"", permanently: false } ) { successful } }";

    private const string ExecuteWorkflowCommand =
        @"mutation { executeWorkflowCommand( input: { commandId: ""{COMMAND_ID}"" item: { itemId: ""{ITEM_ID}"" language: {LANGUAGE} version: {VERSION}  } } ) { successful nextStateId message completed error } }""";

    private const string UpdateItem = @"mutation { updateItem( input: { language: ""{LANGUAGE}"" itemId: ""{ITEM_ID}"" version: {VERSION} fields: [ {FIELDS} ] } ) { item { itemId name path fields(ownFields: true, excludeStandardFields: true) { nodes { name value } } } } }";
    
    private const string AddItemVersion = @"mutation { addItemVersion( input: { itemId: ""{ITEM_ID}"" language: ""{LANGUAGE}"" } ) { item { itemId name path version fields(ownFields: true, excludeStandardFields: true) { nodes { name value } } } } }";
    
    public static string DeleteItemMutation(string itemId)
    {
        return DeleteItem.Replace("{ITEM_ID}", itemId);
    }
    
    public static string ExecuteWorkflowCommandMutation(string commandId, string itemId, string? language, int? version)
    {
        return ExecuteWorkflowCommand
            .Replace("{COMMAND_ID}", commandId)
            .Replace("{ITEM_ID}", itemId)
            .Replace("{LANGUAGE}", language ?? "null")
            .Replace("{VERSION}", version?.ToString() ?? "null");
    }

    public static string UpdateItemMutation(ContentMetadata metadata, List<FieldResponse> fields)
    {
        var fieldsBuilder = new StringBuilder();
        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            fieldsBuilder.Append($"{{ name: \"{field.Name}\", value: \"{field.Value.Replace("\"", "\\\"")}\" }}");
            if (i < fields.Count - 1)
            {
                fieldsBuilder.Append(" ");
            }
        }

        return UpdateItem
            .Replace("{LANGUAGE}", metadata.TargetLanguage)
            .Replace("{ITEM_ID}", metadata.ContentId)
            .Replace("{VERSION}", metadata.Version?.ToString() ?? "null")
            .Replace("{FIELDS}", fieldsBuilder.ToString());
    }
    
    public static string AddItemVersionMutation(string itemId, string language)
    {
        return AddItemVersion
            .Replace("{ITEM_ID}", itemId)
            .Replace("{LANGUAGE}", language);
    }
}