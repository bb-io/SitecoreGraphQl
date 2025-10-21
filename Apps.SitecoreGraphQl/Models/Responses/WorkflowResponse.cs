using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class WorkflowResponse
{
    [Display("Workflow ID"), JsonProperty("workflowId")]
    public string WorkflowId { get; set; } = string.Empty;
    
    [Display("Workflow name"), JsonProperty("displayName")]
    public string Name { get; set; } = string.Empty;
}