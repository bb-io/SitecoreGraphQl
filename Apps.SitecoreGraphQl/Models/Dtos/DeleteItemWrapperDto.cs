using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class DeleteItemWrapperDto
{
    [JsonProperty("deleteItem")]
    public DeleteContentResponse DeleteContent { get; set; } = new();
}
