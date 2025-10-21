using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class GraphQlResponseDto<T>
{
    [JsonProperty("data")]
    public T Data { get; set; } = default!;
    
    [JsonProperty("errors")]
    public List<GraphQlErrorDto> Errors { get; set; } = new();
    
    public string GetErrorMessages()
    {
        return string.Join("; ", Errors.Select(e => e.Message));
    }
}