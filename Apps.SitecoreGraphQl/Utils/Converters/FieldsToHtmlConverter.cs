using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Responses;
using HtmlAgilityPack;

namespace Apps.SitecoreGraphQl.Utils.Converters;

public static class FieldsToHtmlConverter
{
    public static string ConvertToHtml(ContentMetadata contentMetadata, List<FieldResponse> fields)
    {
        var htmlDoc = new HtmlDocument();
        var html = htmlDoc.CreateElement("html");
        var head = htmlDoc.CreateElement("head");
        var body = htmlDoc.CreateElement("body");
        
        htmlDoc.DocumentNode.AppendChild(html);
        html.AppendChild(head);
        html.AppendChild(body);
        
        InjectMetadata(htmlDoc, head, contentMetadata);
        var titleField = GetTitleField(fields);
        if (titleField != null)
        {
            var title = htmlDoc.CreateElement("title");
            title.SetAttributeValue("data-field-name", titleField.Name);
            title.AppendChild(htmlDoc.CreateTextNode(titleField.Value));
            head.AppendChild(title);
        }
        
        ConvertFieldsToBody(htmlDoc, body, fields, titleField?.Name);
        
        return htmlDoc.DocumentNode.OuterHtml;
    }
    
    private static void InjectMetadata(HtmlDocument htmlDoc, HtmlNode head, ContentMetadata contentMetadata)
    {
        var contentIdMeta = htmlDoc.CreateElement("meta");
        contentIdMeta.SetAttributeValue("name", "content-id");
        contentIdMeta.SetAttributeValue("content", contentMetadata.ContentId);
        head.AppendChild(contentIdMeta);
        
        if (contentMetadata.Version.HasValue)
        {
            var versionMeta = htmlDoc.CreateElement("meta");
            versionMeta.SetAttributeValue("name", "version");
            versionMeta.SetAttributeValue("content", contentMetadata.Version.Value.ToString());
            head.AppendChild(versionMeta);
        }
        
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
        var titleFieldNames = new[] { "title", "name", "displayname", "pagetitle", "heading" };
        
        return fields.FirstOrDefault(f => 
            titleFieldNames.Contains(f.Name.ToLowerInvariant()) && 
            !string.IsNullOrWhiteSpace(f.Value));
    }
    
    private static void ConvertFieldsToBody(HtmlDocument htmlDoc, HtmlNode body, List<FieldResponse> fields, string? excludeFieldName)
    {
        foreach (var field in fields)
        {
            if (field.Name.Equals(excludeFieldName, StringComparison.OrdinalIgnoreCase))
                continue;
                
            if (string.IsNullOrWhiteSpace(field.Value))
                continue;
            
            var fieldDiv = htmlDoc.CreateElement("div");
            fieldDiv.SetAttributeValue("data-field-name", field.Name);
            fieldDiv.SetAttributeValue("data-field-type", "content");
            
            try
            {
                var fieldDoc = new HtmlDocument();
                fieldDoc.LoadHtml(field.Value);
                
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
                fieldDiv.AppendChild(htmlDoc.CreateTextNode(field.Value));
            }
            
            body.AppendChild(fieldDiv);
        }
    }
}