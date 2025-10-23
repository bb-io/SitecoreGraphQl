using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class DeleteContentResponse
{
    [JsonProperty("successful")]
    public bool Successful { get; set; }
}