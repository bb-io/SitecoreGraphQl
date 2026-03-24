using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class ContentResponse : BaseContentResponse, IContentOutput
{
    [Display("Content ID"), JsonProperty("itemId")]
    public string Id { get; set; } = string.Empty;
}