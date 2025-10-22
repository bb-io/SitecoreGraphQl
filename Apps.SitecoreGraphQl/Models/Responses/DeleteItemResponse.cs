using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class DeleteItemResponse
{
    [JsonProperty("successful")]
    public bool Successful { get; set; }
}