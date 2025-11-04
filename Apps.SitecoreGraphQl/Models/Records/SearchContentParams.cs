using Apps.SitecoreGraphQl.Models.Dtos;

namespace Apps.SitecoreGraphQl.Models.Records;

public record SearchContentParams(
    string? Language, 
    List<CriteriaDto>? Criteria = null, 
    List<CriteriaDto>? subCriterias = null, 
    List<SortDto>? Sort = null, 
    bool IncludeOnlyOwnFields = false, 
    bool ExcludeStandardFields = false, 
    bool AutoPagination = true);
