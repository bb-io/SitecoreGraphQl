using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class UpdateItemWrapperDto
{
    [JsonProperty("updateItem")]
    public UpdateItemDto UpdateItem { get; set; } = new();
}

public class UpdateItemDto
{
    [JsonProperty("item")]
    public ContentResponse Item { get; set; } = new();
}
