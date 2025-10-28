using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.SitecoreGraphQl.Models.Responses;

public class ContentResponse : IContentOutput
{
    [Display("Content ID"), JsonProperty("itemId")]
    public string Id { get; set; } = string.Empty;
    
    [Display("Content name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Display name")]
    public string DisplayName { get; set; } = string.Empty;
    
    [Display("Path")]
    public string Path { get; set; } = string.Empty;
    
    [Display("Version")]
    public int Version { get; set; }

    [Display("Workflow information"), JsonProperty("workflow")]
    public ItemWorkflowResponse WorkflowInfo { get; set; } = new();

    [Display("Language information"), JsonProperty("language")]
    public LanguageWrapperResponse Language { get; set; }

    [DefinitionIgnore]
    public FieldsResponse Fields { get; set; } = new();
}

public class ItemWorkflowResponse
{
    [Display("Workflow"), JsonProperty("workflow")]
    public WorkflowResponse Workflow { get; set; } = new();
    
    [Display("Workflow state"), JsonProperty("workflowState")]
    public WorkflowStateResponse WorkflowState { get; set; } = new();
}

public class FieldsResponse
{
    public List<FieldResponse> Nodes { get; set; } = new();
}

public class FieldResponse
{
    [Display("Field name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Field value")]
    public string Value { get; set; } = string.Empty;
    
    [Display("Template field")]
    public TemplateFieldResponse TemplateField { get; set; } = new();
}

public class LanguageWrapperResponse
{
    [Display("Language"), JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}

public class SectionResponse
{
    [Display("Section ID"), JsonProperty("itemTemplateSectionId")]
    public string SectionId { get; set; } = string.Empty;
    
    [Display("Section name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Section key")]
    public string Key { get; set; } = string.Empty;
}

public class TemplateFieldResponse
{
    [Display("Field name")]
    public string Name { get; set; } = string.Empty;
    
    [Display("Field type")]
    public string Type { get; set; } = string.Empty;
    
    [Display("Field key")]
    public string Key { get; set; } = string.Empty;
    
    [Display("Field type key")]
    public string TypeKey { get; set; } = string.Empty;
    
    [Display("Field title")]
    public string Title { get; set; } = string.Empty;
    
    [Display("Field tooltip")]
    public string ToolTip { get; set; } = string.Empty;
    
    [Display("Field description")]
    public string Description { get; set; } = string.Empty;
    
    [Display("Section")]
    public SectionResponse Section { get; set; } = new();
}