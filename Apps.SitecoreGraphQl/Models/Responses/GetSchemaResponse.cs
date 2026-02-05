using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class GetSchemaResponse
{
    [Display("Schema JSON")]
    public string SchemaJson { get; set; } = string.Empty;
}