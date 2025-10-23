using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class WorkflowsWrapperDto
{
    [JsonProperty("workflows")]
    public WorkflowsDto Workflows { get; set; } = new();
}

public class WorkflowsDto
{
    [JsonProperty("nodes")]
    public List<WorkflowDto> Nodes { get; set; } = new();
}

public class WorkflowDto
{
    [JsonProperty("workflowId")]
    public string WorkflowId { get; set; } = string.Empty;
    
    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonProperty("initialState")]
    public WorkflowStateDto InitialState { get; set; } = new();
    
    [JsonProperty("states")]
    public WorkflowStatesDto States { get; set; } = new();
}

public class WorkflowStatesDto
{
    [JsonProperty("nodes")]
    public List<WorkflowStateDto> Nodes { get; set; } = new();
}

public class WorkflowStateDto
{
    [JsonProperty("stateId")]
    public string StateId { get; set; } = string.Empty;
    
    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}
