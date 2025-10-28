using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class DownloadContentRequest
{
    [Display("Include child items")]
    public bool? IncludeChildItems { get; set; }
}