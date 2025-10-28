using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class DateFilters
{
    [Display("Created after")]
    public DateTime? CreatedAfter { get; set; }
    
    [Display("Created before")]
    public DateTime? CreatedBefore { get; set; }
    
    [Display("Updated after")]
    public DateTime? UpdatedAfter { get; set; }
    
    [Display("Updated before")]
    public DateTime? UpdatedBefore { get; set; }
}