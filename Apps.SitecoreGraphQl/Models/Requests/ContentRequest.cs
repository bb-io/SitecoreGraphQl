using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class ContentRequest : IDownloadContentInput
{
    [Display("Content ID"), DataSource(typeof(ContentDataSource))]
    public string ContentId { get; set; } = string.Empty;
    
    [Display("Version")]
    public int? Version { get; set; }

    [Display("Language"), DataSource(typeof(LanguageDataSource))]
    public string Language { get; set; } = string.Empty;

    public string GetContentId()
    {
        if(string.IsNullOrEmpty(ContentId))
        {
            throw new PluginMisconfigurationException("Item ID is null or empty, please provide a valid Content ID.");
        }
        
        return ContentId;
    }
}