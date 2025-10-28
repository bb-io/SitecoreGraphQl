using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Events.Models;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Records;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.SitecoreGraphQl.Events;

[PollingEventList("Content")]
public class ContentPollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [PollingEvent("On content created or updated", Description = "Polling event that periodically checks for new new or updated content. If the new or updated content is found, it will be returned as a list of content items.")]
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdatedMultiple)]
    public async Task<PollingEventResponse<DateMemory, SearchContentResponse>> OnContentCreatedOrUpdatedAsync(PollingEventRequest<DateMemory> request,
        [PollingEventParameter] SearchContentRequest searchContentRequest)
    {
        return await ProcessPollingRequest(request, searchContentRequest);
    }

    private async Task<PollingEventResponse<DateMemory, SearchContentResponse>> ProcessPollingRequest(
        PollingEventRequest<DateMemory> request,
        SearchContentRequest searchContentRequest)
    {
        if (request.Memory == null)
        {
            return new()
            {
                Result = null,
                FlyBird = false,
                Memory = new DateMemory
                {
                    LastPollingTime = DateTime.UtcNow
                }
            };
        }

        var criteria = new List<CriteriaDto>();
        if (!string.IsNullOrEmpty(searchContentRequest.RootPath))
        {
            var pathRequest = new Request(CredentialsProviders)
                .AddJsonBody(new
                {
                    query = GraphQlQueries.GetItemByPathQuery(searchContentRequest.RootPath)
                });

            var pathResult = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(pathRequest);
            if (pathResult.Content == null)
            {
                throw new PluginApplicationException(
                    $"Item with path '{searchContentRequest.RootPath}' was not found. Please provide a correct item path.");
            }
            
            criteria.Add(new CriteriaDto
            {
                Field = "_path",
                CriteriaType = "SEARCH",
                Operator = "MUST",
                Value = pathResult.Content.Id
            });
        }
        
        var sinceRange = $"[{request.Memory.LastPollingTime:yyyy-MM-ddTHH:mm:ssZ} TO NOW]";
        criteria.Add(new CriteriaDto
        {
            Field = "__smallupdateddate",
            CriteriaType = "RANGE",
            Operator = "SHOULD",
            Value = sinceRange
        });
        criteria.Add(new CriteriaDto
        {
            Field = "__smallcreateddate",
            CriteriaType = "RANGE",
            Operator = "SHOULD",
            Value = sinceRange
        });
        
        var subCriteria = new List<CriteriaDto>();
        if(searchContentRequest.FieldNames != null && searchContentRequest.FieldValues != null)
        {
            var fieldNames = searchContentRequest.FieldNames.ToList();
            var fieldValues = searchContentRequest.FieldValues.ToList();
            if (fieldNames.Count != fieldValues.Count)
            {
                throw new PluginMisconfigurationException("Field names and field values counts do not match.");
            }

            bool isMainCriteriaEmpty = criteria.Count == 0;
            for (int i = 0; i < fieldNames.Count; i++)
            {
                if (isMainCriteriaEmpty)
                {
                    // If user doesn't provide any main criteria, we add the first field criteria to the main criteria list
                    criteria.Add(new CriteriaDto
                    {
                        Field = fieldNames[i],
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = fieldValues[i]
                    });
                }
                else
                {
                    subCriteria.Add(new CriteriaDto
                    {
                        Field = fieldNames[i],
                        CriteriaType = "WILDCARD",
                        Operator = "MUST",
                        Value = fieldValues[i]
                    });
                }
            }
        }
        
        subCriteria = subCriteria.Count == 0 ? null : subCriteria;
        var searchParams = new SearchContentParams(searchContentRequest.Language, criteria, subCriteria);
        var allItems = await Client.SearchContentAsync(searchParams, CredentialsProviders);
        
        if(searchContentRequest.FieldNames != null && searchContentRequest.FieldValues != null)
        {
            // Filter the results again to ensure all field name/value pairs are matched
            var fieldNames = searchContentRequest.FieldNames.ToList();
            var fieldValues = searchContentRequest.FieldValues.ToList();
            allItems = allItems.Where(item =>
            {
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    var field = item.Fields.Nodes.FirstOrDefault(f => f.Name.Equals(fieldNames[i], StringComparison.OrdinalIgnoreCase));
                    if (field == null || field.Value == null || !field.Value.ToString().Contains(fieldValues[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }).ToList();
        }
        
        return new PollingEventResponse<DateMemory, SearchContentResponse>
        {
            Result = new SearchContentResponse
            {
                Items = allItems
            },
            FlyBird = allItems.Any(),
            Memory = new DateMemory
            {
                LastPollingTime = DateTime.UtcNow
            }
        };
    }
}