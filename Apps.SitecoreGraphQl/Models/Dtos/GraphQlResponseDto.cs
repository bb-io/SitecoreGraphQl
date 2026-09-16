using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class GraphQlResponseDto<T>
{
    [JsonProperty("data")]
    public T Data { get; set; } = default!;
    
    [JsonProperty("errors")]
    public List<GraphQlErrorDto> Errors { get; set; } = new();
    
    public bool HasData()
    {
        return Data switch
        {
            null => false,
            JObject obj => obj.Properties().Any(p => p.Value.Type != JTokenType.Null),
            _ => true
        };
    }

    public string GetErrorMessages()
    {
        return string.Join("; ", Errors.Select(e => e.Message));
    }
}