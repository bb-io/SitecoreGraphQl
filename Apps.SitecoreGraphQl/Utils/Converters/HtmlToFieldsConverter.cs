using Apps.SitecoreGraphQl.Models.Responses;
using Apps.SitecoreGraphQl.Models.Records;
using HtmlAgilityPack;

namespace Apps.SitecoreGraphQl.Utils.Converters;

public static class HtmlToFieldsConverter
{
    public static List<FieldResponse> ConvertToFields(string htmlContent)
    {
        var fields = new List<FieldResponse>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        
        var titleNode = doc.DocumentNode.SelectSingleNode("//title");
        if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
        {
            var fieldName = titleNode.GetAttributeValue("data-field-name", "Title");
            fields.Add(new FieldResponse
            {
                Name = fieldName,
                Value = titleNode.InnerText
            });
        }
        
        var fieldDivs = doc.DocumentNode.SelectNodes("//body//div[@data-field-name]");
        if (fieldDivs != null)
        {
            foreach (var fieldDiv in fieldDivs)
            {
                var fieldName = fieldDiv.GetAttributeValue("data-field-name", string.Empty);
                if (!string.IsNullOrEmpty(fieldName))
                {
                    fields.Add(new FieldResponse
                    {
                        Name = fieldName,
                        Value = fieldDiv.InnerHtml
                    });
                }
            }
        }
        
        return fields;
    }
    
    public static ContentMetadata ExtractMetadata(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        
        var contentId = string.Empty;
        int? version = null;
        string? sourceLanguage = null;
        
        var contentIdMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='content-id']");
        if (contentIdMeta != null)
        {
            contentId = contentIdMeta.GetAttributeValue("content", string.Empty);
        }
        
        var versionMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='version']");
        if (versionMeta != null)
        {
            var versionValue = versionMeta.GetAttributeValue("content", string.Empty);
            if (int.TryParse(versionValue, out var parsedVersion))
            {
                version = parsedVersion;
            }
        }
        
        var languageMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='source-language']");
        if (languageMeta != null)
        {
            sourceLanguage = languageMeta.GetAttributeValue("content", string.Empty);
            if (string.IsNullOrWhiteSpace(sourceLanguage))
            {
                sourceLanguage = null;
            }
        }
        
        return new ContentMetadata(contentId, version, sourceLanguage);
    }
}