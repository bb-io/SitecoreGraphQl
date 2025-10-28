using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class FilteringOptions
{
    [Display("Include only own fields")]
    public bool? IncludeOnlyOwnFields { get; set; } = false;
    
    [Display("Exclude standard fields")]
    public bool? ExcludeStandardFields { get; set; } = false;
    
    [Display("Exclude fields where Value property equals")] // field.value
    public IEnumerable<string>? Value { get; set; }

    [Display("Exclude fields where Type property equals")] // field.templateField.type
    public IEnumerable<string>? Type { get; set; }

    [Display("Exclude fields where TypeKey property equals")] // field.templateField.typeKey
    public IEnumerable<string>? TypeKey { get; set; }

    [Display("Exclude fields where Description property equals")] // field.templateField.description
    public IEnumerable<string>? Description { get; set; }

    [Display("Exclude fields where Title property equals")] // field.templateField.title
    public IEnumerable<string>? Title { get; set; }

    [Display("Exclude fields where Tooltip property equals")]
    public IEnumerable<string>? ToolTip { get; set; }

    [Display("Exclude fields where Name property equals")] // field.templateField.name
    public IEnumerable<string>? Name { get; set; }

    [Display("Exclude fields where Key property equals")] // field.templateField.key
    public IEnumerable<string>? Key { get; set; }

    [Display("Exclude fields where Section property equals")] // field.templateField.section.name
    public IEnumerable<string>? Section { get; set; }

    public List<FieldResponse> ApplyFilteringOptions(List<FieldResponse> originalFields)
    {
        var filteredFields = originalFields;

        if (Value != null)
        {
            filteredFields = filteredFields.Where(x => !Value.Contains(x.Value)).ToList();
        }

        if (Type != null)
        {
            filteredFields = filteredFields.Where(x => !Type.Contains(x.TemplateField.Type)).ToList();
        }

        if (TypeKey != null)
        {
            filteredFields = filteredFields.Where(x => !TypeKey.Contains(x.TemplateField.TypeKey)).ToList();
        }

        if (Description != null)
        {
            filteredFields = filteredFields.Where(x => !Description.Contains(x.TemplateField.Description)).ToList();
        }

        if (Title != null)
        {
            filteredFields = filteredFields.Where(x => !Title.Contains(x.TemplateField.Title)).ToList();
        }

        if (ToolTip != null)
        {
            filteredFields = filteredFields.Where(x => !ToolTip.Contains(x.TemplateField.ToolTip)).ToList();
        }

        if (Name != null)
        {
            filteredFields = filteredFields.Where(x => !Name.Contains(x.TemplateField.Name)).ToList();
        }

        if (Key != null)
        {
            filteredFields = filteredFields.Where(x => !Key.Contains(x.TemplateField.Key)).ToList();
        }

        if (Section != null)
        {
            filteredFields = filteredFields.Where(x => !Section.Contains(x.TemplateField.Section.Name)).ToList();
        }

        return filteredFields;
    }
}