namespace Apps.SitecoreGraphQl.Models.Dtos;

public class CriteriaDto
{
    public string Field { get; set; } = string.Empty;
    public string CriteriaType { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
