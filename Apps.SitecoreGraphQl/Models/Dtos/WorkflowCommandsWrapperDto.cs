using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class WorkflowCommandsWrapperDto
{
    [JsonProperty("workflow")]
    public WorkflowWithCommandsDto Workflow { get; set; } = new();
}

public class WorkflowWithCommandsDto
{
    [JsonProperty("workflowId")]
    public string WorkflowId { get; set; } = string.Empty;
    
    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonProperty("commands")]
    public WorkflowCommandsConnectionDto Commands { get; set; } = new();
}

public class WorkflowCommandsConnectionDto
{
    [JsonProperty("nodes")]
    public List<WorkflowCommandDto> Nodes { get; set; } = new();
}
