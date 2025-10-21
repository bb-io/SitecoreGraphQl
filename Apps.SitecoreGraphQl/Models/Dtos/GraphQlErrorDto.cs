using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class GraphQlErrorDto
{
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
}