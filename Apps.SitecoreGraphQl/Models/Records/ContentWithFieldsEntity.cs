using Apps.SitecoreGraphQl.Models.Responses;

namespace Apps.SitecoreGraphQl.Models.Records;

public record ContentWithFieldsEntity(string ContentId, int Version, string SourceLanguage, List<FieldResponse> Fields, bool IsRootContent = false);