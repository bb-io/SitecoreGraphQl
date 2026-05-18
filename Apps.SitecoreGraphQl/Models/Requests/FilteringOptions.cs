using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class FilteringOptions
{
    [Display("Exclude fields", Description = "Field names to exclude from the downloaded HTML file. Use this to remove non-translatable fields (e.g. IsVerifiedStyle, Value, SortOrder).")]
    public IEnumerable<string>? ExcludeFieldNames { get; set; }

    public List<FieldResponse> ApplyFilteringOptions(List<FieldResponse> originalFields)
    {
        if (ExcludeFieldNames == null)
            return originalFields;

        var excludeSet = new HashSet<string>(ExcludeFieldNames, StringComparer.OrdinalIgnoreCase);
        return originalFields.Where(x => !excludeSet.Contains(x.Name)).ToList();
    }
}
