using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class DownloadContentRequest
{
    [Display("Include child items")]
    public bool? IncludeChildItems { get; set; }
    
    [Display("Field names", Description = "Use this field to specify which child items to include in HTML file")]
    public IEnumerable<string>? FieldNames { get; set; }
    
    [Display("Field values", Description = "Use this field to specify which child items to include in HTML file. This will allow you to include only specific child items based on field values (for example include only items where 'status:translate').")]
    public IEnumerable<string>? FieldValues { get; set; }
}