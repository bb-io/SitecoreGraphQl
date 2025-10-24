using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Dtos;
using System.Text;

namespace Apps.SitecoreGraphQl.Constants;

public static class GraphQlQueries
{
    private const string GetLanguages = "query { languages { nodes { iso name displayName } } }";
    
    private const string GetItemById = @"query { item( where: { database: ""master"", itemId: ""{ITEM_ID}"", language: {LANGUAGE}, version: {VERSION} }) { itemId name path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: true, excludeStandardFields: true) { nodes {  name value } } } }";
    
    private const string GetItemByPath = @"query { item( where: { database: ""master"", path: ""{ITEM_PATH}"" }) { itemId name path version language { name } workflow { workflowState { stateId displayName } workflow { workflowId displayName } } fields(ownFields: true, excludeStandardFields: true) { nodes { name value } } } }";
    
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
      path
      version
      createdDate
      updatedDate
      innerItem {
        itemId
        name
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
        fields(ownFields: true, excludeStandardFields: true) {
          nodes {
            name
            value
          }
        }
      }
    }
  }
}";

    private const string SearchItemsWithFilters = @"query SearchItems($language: String, $pageIndex: Int, $pageSize: Int) {
  search(
    query: {
      language: $language
      latestVersionOnly: true
      paging: {
        pageIndex: $pageIndex
        pageSize: $pageSize
      }
      searchStatement: {
        criteria: [
          {CRITERIA}
        ]
      }
      sort: [ {SORT} ]
    }
  ) {
    totalCount
    results {
      itemId
      name
      path
      version
      createdDate
      updatedDate
      innerItem {
        itemId
        name
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
        fields(ownFields: true, excludeStandardFields: true) {
          nodes {
            name
            value
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
    
    public static string GetItemByIdQuery(ContentRequest contentRequest)
    {
        var itemId = contentRequest.GetContentId();
        var finalQuery = GetItemById
            .Replace("{ITEM_ID}", itemId)
            .Replace("{LANGUAGE}", contentRequest.Language == null ? "null" : $"\"{contentRequest.Language}\"")
            .Replace("{VERSION}", contentRequest.Version.HasValue ? $"{contentRequest.Version.Value}" : "null");
        
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
    
    public static string SearchItemsWithCriteriasQuery(List<CriteriaDto> criterias, List<SortDto>? sorts = null)
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
        
        if(sorts == null)
        {
            return SearchItemsWithFilters.Replace("{CRITERIA}", criteriaBuilder.ToString())
              .Replace("{SORT}", string.Empty);
        }
        
        var sortBuilder = new StringBuilder();
        for (int i = 0; i < sorts.Count; i++)
        {
            var sort = sorts[i];
            sortBuilder.Append($"{{ field: \"{sort.Field}\", direction: {sort.Direction} }}");
            if (i < sorts.Count - 1)
            {
                sortBuilder.Append(", ");
            }
        }
        
        return SearchItemsWithFilters
            .Replace("{CRITERIA}", criteriaBuilder.ToString())
            .Replace("{SORT}", sortBuilder.ToString());
    }
}