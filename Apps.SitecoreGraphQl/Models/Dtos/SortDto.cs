using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class SortDto
{
    [JsonProperty("field")]
    public string Field { get; set; } = string.Empty;
    
    [JsonProperty("direction")]
    public string Direction { get; set; } = "ASCENDING";
}