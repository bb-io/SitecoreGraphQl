using Apps.SitecoreGraphQl.Models.Responses;
using Apps.SitecoreGraphQl.Models.Records;
using HtmlAgilityPack;

namespace Apps.SitecoreGraphQl.Utils.Converters;

public static class HtmlToFieldsConverter
{
    public static List<ContentWithFieldsEntity> ConvertToContentEntities(string htmlContent)
    {
        var entities = new List<ContentWithFieldsEntity>();
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        
        var metadata = ExtractMetadata(htmlContent);
        var rootFields = ExtractRootFields(doc);
        if (rootFields.Count > 0)
        {
            entities.Add(new ContentWithFieldsEntity(
                metadata.ContentId,
                metadata.Version ?? 1,
                metadata.SourceLanguage ?? "en",
                rootFields,
                IsRootContent: true
            ));
        }
        
        // Extract child items
        var childDivs = doc.DocumentNode.SelectNodes("//body/div[@data-content-id]");
        if (childDivs != null)
        {
            foreach (var childDiv in childDivs)
            {
                var contentId = childDiv.GetAttributeValue("data-content-id", string.Empty);
                var language = childDiv.GetAttributeValue("data-language", metadata.SourceLanguage ?? "en");
                var versionStr = childDiv.GetAttributeValue("data-version", "1");
                int.TryParse(versionStr, out var version);
                
                var childFields = ExtractFieldsFromNode(childDiv);
                if (!string.IsNullOrEmpty(contentId) && childFields.Count > 0)
                {
                    entities.Add(new ContentWithFieldsEntity(
                        contentId,
                        version,
                        language,
                        childFields,
                        IsRootContent: false
                    ));
                }
            }
        }
        
        return entities;
    }
    
    public static ContentMetadata ExtractMetadata(string htmlContent)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        
        var contentId = string.Empty;
        var rootContentId = string.Empty;
        int? version = null;
        string? sourceLanguage = null;
        
        var rootContentIdMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='root-content-id']");
        if (rootContentIdMeta != null)
        {
            rootContentId = rootContentIdMeta.GetAttributeValue("content", string.Empty);
        }
        
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
        
        return new ContentMetadata(contentId, version, sourceLanguage, RootContentId: rootContentId);
    }
    
    private static List<FieldResponse> ExtractRootFields(HtmlDocument doc)
    {
        var fields = new List<FieldResponse>();
        
        // Extract title field
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
        
        // Extract fields from body that are not child content divs
        var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
        if (bodyNode != null)
        {
            foreach (var childNode in bodyNode.ChildNodes)
            {
                if (childNode.NodeType != HtmlNodeType.Element || childNode.Name != "div")
                    continue;
                
                // Skip child content divs (they have data-content-id attribute)
                if (childNode.Attributes.Contains("data-content-id"))
                    continue;
                
                var fieldName = childNode.GetAttributeValue("data-field-name", string.Empty);
                if (!string.IsNullOrEmpty(fieldName))
                {
                    fields.Add(new FieldResponse
                    {
                        Name = fieldName,
                        Value = childNode.InnerHtml
                    });
                }
            }
        }
        
        return fields;
    }
    
    private static List<FieldResponse> ExtractFieldsFromNode(HtmlNode node)
    {
        var fields = new List<FieldResponse>();
        var fieldDivs = node.SelectNodes(".//div[@data-field-name]");
        
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
}