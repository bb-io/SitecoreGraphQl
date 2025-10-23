using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class UploadContentRequest :IUploadContentInput
{
    public FileReference Content { get; set; } = default!;
    
    [Display("Content ID")]
    public string? ContentId { get; set; }
    
    [Display("Version")]
    public int? Version { get; set; }

    [Display("Target language"), DataSource(typeof(LanguageDataSource))]
    public string Locale { get; set; } = string.Empty;
}