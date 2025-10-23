using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class WorkflowCommandDto
{
    [JsonProperty("commandId")]
    public string CommandId { get; set; } = string.Empty;
    
    [JsonProperty("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}
