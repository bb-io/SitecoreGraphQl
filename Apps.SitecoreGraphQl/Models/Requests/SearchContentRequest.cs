using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class SearchContentRequest
{
    [Display("Language"), DataSource(typeof(LanguageDataSource))]
    public string? Language { get; set; }
    
    [Display("Root path")]
    public string? RootPath { get; set; }
}