using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Responses;
using HtmlAgilityPack;

namespace Apps.SitecoreGraphQl.Utils.Converters;

public static class FieldsToHtmlConverter
{
    public static string ConvertToHtml(ContentMetadata contentMetadata, List<FieldResponse> fields)
    {
        var htmlDoc = new HtmlDocument();
        
        // Create HTML structure
        var html = htmlDoc.CreateElement("html");
        var head = htmlDoc.CreateElement("head");
        var body = htmlDoc.CreateElement("body");
        
        htmlDoc.DocumentNode.AppendChild(html);
        html.AppendChild(head);
        html.AppendChild(body);
        
        // Inject ContentMetadata properties into HTML file as meta in the header
        InjectMetadata(htmlDoc, head, contentMetadata);
          // Get title or name field and place in title tag
        var titleField = GetTitleField(fields);
        if (titleField != null)
        {
            var title = htmlDoc.CreateElement("title");
            title.AppendChild(htmlDoc.CreateTextNode(titleField.Value));
            head.AppendChild(title);
        }
        
        // Convert remaining fields to body content
        ConvertFieldsToBody(htmlDoc, body, fields, titleField?.Name);
        
        return htmlDoc.DocumentNode.OuterHtml;
    }
    
    private static void InjectMetadata(HtmlDocument htmlDoc, HtmlNode head, ContentMetadata contentMetadata)
    {
        // Content ID
        var contentIdMeta = htmlDoc.CreateElement("meta");
        contentIdMeta.SetAttributeValue("name", "content-id");
        contentIdMeta.SetAttributeValue("content", contentMetadata.ContentId);
        head.AppendChild(contentIdMeta);
        
        // Version
        if (contentMetadata.Version.HasValue)
        {
            var versionMeta = htmlDoc.CreateElement("meta");
            versionMeta.SetAttributeValue("name", "version");
            versionMeta.SetAttributeValue("content", contentMetadata.Version.Value.ToString());
            head.AppendChild(versionMeta);
        }
        
        // Source Language
        if (!string.IsNullOrEmpty(contentMetadata.SourceLanguage))
        {
            var languageMeta = htmlDoc.CreateElement("meta");
            languageMeta.SetAttributeValue("name", "source-language");
            languageMeta.SetAttributeValue("content", contentMetadata.SourceLanguage);
            head.AppendChild(languageMeta);
        }
    }
    
    private static FieldResponse? GetTitleField(List<FieldResponse> fields)
    {
        // Look for common title/name field names
        var titleFieldNames = new[] { "title", "name", "displayname", "pagetitle", "heading" };
        
        return fields.FirstOrDefault(f => 
            titleFieldNames.Contains(f.Name.ToLowerInvariant()) && 
            !string.IsNullOrWhiteSpace(f.Value));
    }
    
    private static void ConvertFieldsToBody(HtmlDocument htmlDoc, HtmlNode body, List<FieldResponse> fields, string? excludeFieldName)
    {
        foreach (var field in fields)
        {
            // Skip the title field as it's already in the title tag
            if (field.Name.Equals(excludeFieldName, StringComparison.OrdinalIgnoreCase))
                continue;
                
            if (string.IsNullOrWhiteSpace(field.Value))
                continue;
            
            // Create a div container for each field with data attributes for restoration
            var fieldDiv = htmlDoc.CreateElement("div");
            fieldDiv.SetAttributeValue("data-field-name", field.Name);
            fieldDiv.SetAttributeValue("data-field-type", "content");
              // Try to parse the field value as HTML, otherwise treat as plain text
            try
            {
                var fieldDoc = new HtmlDocument();
                fieldDoc.LoadHtml(field.Value);
                
                // If it's valid HTML content, append the parsed nodes
                if (fieldDoc.DocumentNode.HasChildNodes)
                {
                    foreach (var childNode in fieldDoc.DocumentNode.ChildNodes)
                    {
                        var clonedNode = childNode.CloneNode(true);
                        fieldDiv.AppendChild(clonedNode);
                    }
                }
                else
                {
                    fieldDiv.AppendChild(htmlDoc.CreateTextNode(field.Value));
                }
            }
            catch
            {
                // If parsing fails, treat as plain text
                fieldDiv.AppendChild(htmlDoc.CreateTextNode(field.Value));
            }
            
            body.AppendChild(fieldDiv);
        }
    }
}