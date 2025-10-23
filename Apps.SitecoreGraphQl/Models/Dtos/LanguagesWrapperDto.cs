namespace Apps.SitecoreGraphQl.Models.Dtos;

public class LanguagesWrapperDto
{
    public NodesDto<LanguageDto> Languages { get; set; } = new();
}