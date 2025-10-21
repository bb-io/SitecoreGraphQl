using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class ItemWrapperDto
{
    [JsonProperty("item")]
    public ItemResponse Item { get; set; } = new();
}