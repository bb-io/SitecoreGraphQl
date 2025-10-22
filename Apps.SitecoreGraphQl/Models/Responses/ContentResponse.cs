using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class ContentResponse
{
    [Display("Item ID")]
    public string ItemId { get; set; } = string.Empty;
    
    [Display("Item name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Path")]
    public string Path { get; set; } = string.Empty;
    
    [Display("Version")]
    public int Version { get; set; }

    [Display("Workflow information"), JsonProperty("workflow")]
    public ItemWorkflowResponse WorkflowInfo { get; set; } = new();

    [DefinitionIgnore]
    public FieldsResponse Fields { get; set; } = new();
}

public class ItemWorkflowResponse
{
    [Display("Workflow"), JsonProperty("workflow")]
    public WorkflowResponse Workflow { get; set; } = new();
    
    [Display("Workflow state"), JsonProperty("workflowState")]
    public WorkflowStateResponse WorkflowState { get; set; } = new();
}

public class FieldsResponse
{
    public List<FieldResponse> Nodes { get; set; } = new();
}

public class FieldResponse
{
    [Display("Field name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Field value")]
    public string Value { get; set; } = string.Empty;
}