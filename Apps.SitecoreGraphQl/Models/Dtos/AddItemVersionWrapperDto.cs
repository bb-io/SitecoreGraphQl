using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class AddItemVersionWrapperDto
{
    [JsonProperty("addItemVersion")]
    public AddItemVersionDto AddItemVersion { get; set; } = new();
}

public class AddItemVersionDto
{
    [JsonProperty("item")]
    public ContentResponse Item { get; set; } = new();
}
