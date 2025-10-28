using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Responses;
using HtmlAgilityPack;

namespace Apps.SitecoreGraphQl.Utils.Converters;

public static class FieldsToHtmlConverter
{
    private const string MetaRootContentId = "root-content-id";
    private const string MetaContentId = "content-id";
    private const string MetaVersion = "version";
    private const string MetaSourceLanguage = "source-language";
    
    private const string AttrContentId = "data-content-id";
    private const string AttrLanguage = "data-language";
    private const string AttrVersion = "data-version";
    private const string AttrFieldName = "data-field-name";
    private const string AttrFieldType = "data-field-type";
    
    public static string ConvertToHtml(ContentMetadata rootContentMetadata, List<ContentWithFieldsEntity> fieldsEntities)
    {
        var htmlDoc = new HtmlDocument();
        var html = htmlDoc.CreateElement("html");
        var head = htmlDoc.CreateElement("head");
        var body = htmlDoc.CreateElement("body");
        
        htmlDoc.DocumentNode.AppendChild(html);
        html.AppendChild(head);
        html.AppendChild(body);
        
        InjectMetadata(htmlDoc, head, rootContentMetadata);
        var rootEntity = fieldsEntities.FirstOrDefault(e => e.IsRootContent)
            ?? throw new InvalidOperationException("Root content entity not found.");
        var titleField = GetTitleField(rootEntity.Fields);
        if (titleField != null)
        {
            var title = htmlDoc.CreateElement("title");
            title.SetAttributeValue(AttrFieldName, titleField.Name);
            title.AppendChild(htmlDoc.CreateTextNode(titleField.Value));
            head.AppendChild(title);
        }
        
        ConvertFieldsToBody(htmlDoc, body, rootEntity.Fields, titleField?.Name);
        
        var childEntities = fieldsEntities.Where(e => !e.IsRootContent).ToList();
        foreach (var childEntity in childEntities)
        {
            var childDiv = htmlDoc.CreateElement("div");
            childDiv.SetAttributeValue(AttrContentId, childEntity.ContentId);
            childDiv.SetAttributeValue(AttrLanguage, childEntity.SourceLanguage);
            childDiv.SetAttributeValue(AttrVersion, childEntity.Version.ToString());
            
            ConvertFieldsToBody(htmlDoc, childDiv, childEntity.Fields, null);
            body.AppendChild(childDiv);
        }
        
        return htmlDoc.DocumentNode.OuterHtml;
    }
    
    private static void InjectMetadata(HtmlDocument htmlDoc, HtmlNode head, ContentMetadata contentMetadata)
    {
        var rootContentIdMeta = htmlDoc.CreateElement("meta");
        rootContentIdMeta.SetAttributeValue("name", MetaRootContentId);
        rootContentIdMeta.SetAttributeValue("content", contentMetadata.RootContentId ?? contentMetadata.ContentId);
        head.AppendChild(rootContentIdMeta);
        
        var contentIdMeta = htmlDoc.CreateElement("meta");
        contentIdMeta.SetAttributeValue("name", MetaContentId);
        contentIdMeta.SetAttributeValue("content", contentMetadata.ContentId);
        head.AppendChild(contentIdMeta);
        
        if (contentMetadata.Version.HasValue)
        {
            var versionMeta = htmlDoc.CreateElement("meta");
            versionMeta.SetAttributeValue("name", MetaVersion);
            versionMeta.SetAttributeValue("content", contentMetadata.Version.Value.ToString());
            head.AppendChild(versionMeta);
        }
        
        if (!string.IsNullOrEmpty(contentMetadata.SourceLanguage))
        {
            var languageMeta = htmlDoc.CreateElement("meta");
            languageMeta.SetAttributeValue("name", MetaSourceLanguage);
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
            fieldDiv.SetAttributeValue(AttrFieldName, field.Name);
            
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