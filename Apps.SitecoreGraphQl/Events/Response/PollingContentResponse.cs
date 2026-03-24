using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Events.Response;

public class PollingContentResponse(ContentResponse contentResponse) : BaseContentResponse, IDownloadContentInput
{
    [Display("Content ID"), JsonProperty("itemId")]
    public string ContentId { get; set; } = contentResponse.Id;
}
