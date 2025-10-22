using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class ExecuteCommandResponse
{
    [Display("Next state ID")]
    public string NextStateId { get; set; } = string.Empty;
    
    [Display("Message")]
    public string Message { get; set; } = string.Empty;

    [Display("Completed")]
    public bool Completed { get; set; }
    
    [DefinitionIgnore]
    public bool Successful { get; set; }

    [DefinitionIgnore]
    public string Error { get; set; } = string.Empty;
}