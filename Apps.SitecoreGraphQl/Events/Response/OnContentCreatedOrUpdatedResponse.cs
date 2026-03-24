using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.SitecoreGraphQl.Events.Response;

public record OnContentCreatedOrUpdatedResponse(List<PollingContentResponse> Items) 
    : IMultiDownloadableContentOutput<PollingContentResponse>
{
    public List<PollingContentResponse> Items { get; set; } = Items;
}
