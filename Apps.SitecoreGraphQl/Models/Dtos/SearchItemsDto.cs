using Apps.SitecoreGraphQl.Models.Responses;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Dtos;

public class SearchItemsWrapperDto
{
    [JsonProperty("search")]
    public SearchItemsDto Search { get; set; } = new();
}

public class SearchItemsDto
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonProperty("results")]
    public List<SearchItemResultDto> Results { get; set; } = new();
}

public class SearchItemResultDto
{
    [JsonProperty("itemId")]
    public string ItemId { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;
    
    [JsonProperty("version")]
    public int Version { get; set; }
    
    [JsonProperty("createdDate")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("updatedDate")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonProperty("innerItem")]
    public ContentResponse? InnerItem { get; set; }
}
