using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class ExecuteCommandWrapperDto
{
    [JsonProperty("executeWorkflowCommand")]
    public ExecuteCommandResponse ExecuteWorkflowCommand { get; set; } = new();
}
