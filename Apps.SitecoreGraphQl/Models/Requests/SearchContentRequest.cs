using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class SearchContentRequest
{
    [Display("Language"), DataSource(typeof(LanguageDataSource))]
    public string Language { get; set; } = string.Empty;
    
    [Display("Root path")]
    public string? RootPath { get; set; }

    [Display("Field names")]
    public IEnumerable<string>? FieldNames { get; set; }
    
    [Display("Field values")]
    public IEnumerable<string>? FieldValues { get; set; }

    [Display("Use exact field value filtering", Description = "When enabled, Sitecore is used only to narrow the search scope, and field values are matched locally using exact normalized comparison.")]
    public bool? UseExactFieldValueFiltering { get; set; }
}
