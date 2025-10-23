using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class SearchContentResponse : ISearchContentOutput<ContentResponse>
{
    [Display("Content collection")]
    public IEnumerable<ContentResponse> Items { get; set; } = new List<ContentResponse>();
}