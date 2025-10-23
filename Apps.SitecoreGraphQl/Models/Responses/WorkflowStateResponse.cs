using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class WorkflowStateResponse
{
    [Display("Workflow state ID"), JsonProperty("stateId")]
    public string WorkflowStateId { get; set; } = string.Empty;
    
    [Display("Workflow state name"), JsonProperty("displayName")]
    public string Name { get; set; } = string.Empty;
}