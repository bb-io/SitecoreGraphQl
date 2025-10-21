using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class RestErrorDto
{
    [JsonProperty("error")]
    public string Error { get; set; } = string.Empty;
    
    [JsonProperty("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;
}