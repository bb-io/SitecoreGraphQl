using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Dtos;
using System.Text;

namespace Apps.SitecoreGraphQl.Constants;

public static class GraphQlQueries
{
    private const string GetLanguages = "query { languages { nodes { iso name displayName } } }";
    
    private const string GetItemById = @"query { item( where: { database: ""master"", itemId: ""{ITEM_ID}"", language: {LANGUAGE}, version: {VERSION} }) { itemId name displayName path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: {OWN_FIELDS}, excludeStandardFields: {EXCLUDE_STANDARD}) { nodes { name value templateField { name type key typeKey title(language: {LANGUAGE}) toolTip(language: {LANGUAGE}) description(language: {LANGUAGE}) section { itemTemplateSectionId key name } } } } } }";
    
    private const string GetItemByPath = @"query { item( where: { database: ""master"", path: ""{ITEM_PATH}"" }) { itemId name displayName path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: false, excludeStandardFields: false) { nodes { name value templateField { name type key typeKey section { itemTemplateSectionId key name } } } } } }";
    
    private const string GetWorkflowCommands = @"query GetWorkflowCommands( $workflowId: String!, $stateId: String! ) { workflow(where: { workflowId: $workflowId }) { workflowId displayName commands(query: { stateId: $stateId }) { nodes { commandId displayName } pageInfo { hasNextPage endCursor } } } }";
    
    private const string SearchWorkflows = @"query { workflows { nodes { workflowId displayName initialState { stateId displayName } states { nodes { stateId displayName } } } } }";
    
    private const string SearchItems = @"query SearchItems($language: String, $pageIndex: Int, $pageSize: Int) {
  search(
    query: {
      language: $language
      latestVersionOnly: true
      paging: {
        pageIndex: $pageIndex
        pageSize: $pageSize
      }
    }
  ) {
    totalCount
    results {
      itemId
      name
      displayName
      path
      version
      createdDate
      updatedDate
      innerItem {
        itemId
        name
        displayName
        path
        version
        language {
          name
        }
        workflow {
          workflowState {
            stateId
            displayName
          }
          workflow {
            workflowId
            displayName
          }
        }
        fields(ownFields: false, excludeStandardFields: false) {
          nodes {
            name
            value
            templateField {
              name
              type
              key
              typeKey
              section {
                itemTemplateSectionId
                key
                name
              }
            }
          }
        }
      }
    }
  }
}";

    private const string SearchItemsWithFilters = @"query SearchItems($language: String!, $pageIndex: Int, $pageSize: Int) {
  search(
    query: {
      language: $language
      latestVersionOnly: true
      paging: {
        pageIndex: $pageIndex
        pageSize: $pageSize
      }
      filterStatement: {
        criteria: [
          {CRITERIA}
        ]
        {SUBSTATEMENTS}
      }
      sort: [ {SORT} ]
    }
  ) {
    totalCount
    results {
      itemId
      name
      displayName
      path
      version
      createdDate
      updatedDate
      innerItem {
        itemId
        name
        displayName
        path
        version
        language {
          name
        }
        workflow {
          workflowState {
            stateId
            displayName
          }
          workflow {
            workflowId
            displayName
          }
        }
        fields(ownFields: {OWN_FIELDS}, excludeStandardFields: {EXCLUDE_STANDARD}) {
          nodes {
            name
            value
            templateField {
              name
              type
              key
              typeKey
              title(language: $language)
              toolTip(language: $language)
              description(language: $language)
              section {
                itemTemplateSectionId
                key
                name
              }
            }
          }
        }
      }
    }
  }
}";

    public static string GetLanguagesQuery()
    {
        return GetLanguages;
    }
    
    public static string GetItemByIdQuery(ContentRequest contentRequest, bool ownFields = false, bool excludeStandardFields = false)
    {
        var itemId = contentRequest.GetContentId();
        var finalQuery = GetItemById
            .Replace("{ITEM_ID}", itemId)
            .Replace("{LANGUAGE}", contentRequest.Language == null ? "null" : $"\"{contentRequest.Language}\"")
            .Replace("{VERSION}", contentRequest.Version.HasValue ? $"{contentRequest.Version.Value}" : "null")
            .Replace("{OWN_FIELDS}", ownFields.ToString().ToLower())
            .Replace("{EXCLUDE_STANDARD}", excludeStandardFields.ToString().ToLower());
        
        return finalQuery;
    }
    
    public static string GetItemByPathQuery(string itemPath)
    {
        return GetItemByPath.Replace("{ITEM_PATH}", itemPath);
    }
    
    public static string GetWorkflowCommandsQuery()
    {
        return GetWorkflowCommands;
    }
    
    public static string SearchWorkflowsQuery()
    {
        return SearchWorkflows;
    }
    
    public static string SearchItemsQuery()
    {
        return SearchItems;
    }
    
    public static string SearchItemsWithCriteriasQuery(List<CriteriaDto> criterias, List<CriteriaDto>? subCriterias = null, List<SortDto>? sorts = null, bool ownFields = false, bool excludeStandardFields = false)
    {
        var criteriaBuilder = new StringBuilder();
        for (int i = 0; i < criterias.Count; i++)
        {
            var criteria = criterias[i];
            criteriaBuilder.Append($"{{ field: \"{criteria.Field}\", criteriaType: {criteria.CriteriaType}, operator: {criteria.Operator}, value: \"{criteria.Value}\" }}");
            if (i < criterias.Count - 1)
            {
                criteriaBuilder.Append(", ");
            }
        }
        
        var subStatementsBuilder = new StringBuilder();
        if (subCriterias != null && subCriterias.Count > 0)
        {
            subStatementsBuilder.Append("subStatements: [ { operator: MUST, criteria: [ ");
            for (int i = 0; i < subCriterias.Count; i++)
            {
                var subCriteria = subCriterias[i];
                subStatementsBuilder.Append($"{{ field: \"{subCriteria.Field}\", criteriaType: {subCriteria.CriteriaType}, operator: {subCriteria.Operator}, value: \"{subCriteria.Value}\" }}");
                if (i < subCriterias.Count - 1)
                {
                    subStatementsBuilder.Append(", ");
                }
            }
            subStatementsBuilder.Append(" ] } ]");
        }
        
        var sortBuilder = new StringBuilder();
        if (sorts != null && sorts.Count > 0)
        {
            for (int i = 0; i < sorts.Count; i++)
            {
                var sort = sorts[i];
                sortBuilder.Append($"{{ field: \"{sort.Field}\", direction: {sort.Direction} }}");
                if (i < sorts.Count - 1)
                {
                    sortBuilder.Append(", ");
                }
            }
        }
        
        return SearchItemsWithFilters
            .Replace("{CRITERIA}", criteriaBuilder.ToString())
            .Replace("{SUBSTATEMENTS}", subStatementsBuilder.Length > 0 ? subStatementsBuilder.ToString() : string.Empty)
            .Replace("{SORT}", sortBuilder.ToString())
            .Replace("{OWN_FIELDS}", ownFields.ToString().ToLower())
            .Replace("{EXCLUDE_STANDARD}", excludeStandardFields.ToString().ToLower());
    }
}