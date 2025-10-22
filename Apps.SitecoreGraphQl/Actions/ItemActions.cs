using Apps.SitecoreGraphQl.Api;
using Apps.SitecoreGraphQl.Constants;
using Apps.SitecoreGraphQl.Models.Dtos;
using Apps.SitecoreGraphQl.Models.Requests;
using Apps.SitecoreGraphQl.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.SitecoreGraphQl.Actions;

[ActionList("Items")]
public class ItemActions(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [Action("Get item", Description = "Get an item by its ID")]
    public async Task<ItemResponse> GetItem([ActionParameter] ItemRequest itemRequest)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlQueries.GetItemByIdQuery(itemRequest.GetItemId())
            });

        var item = await Client.ExecuteGraphQlWithErrorHandling<ItemWrapperDto>(apiRequest);
        if (item.Item == null)
        {
            throw new PluginApplicationException(
                $"Item with ID {itemRequest.ItemId} was not found. Please verify the ID and try again.");
        }

        return item.Item;
    }
    
    [Action("Delete item", Description = "Delete an item by its ID")]
    public async Task DeleteItem([ActionParameter] ItemRequest itemRequest)
    {
        var apiRequest = new Request(CredentialsProviders)
            .AddJsonBody(new
            {
                query = GraphQlMutations.DeleteItemMutation(itemRequest.GetItemId())
            });

        var result = await Client.ExecuteGraphQlWithErrorHandling<DeleteItemWrapperDto>(apiRequest);
        if (!result.DeleteItem.Successful)
        {
            throw new PluginApplicationException(
                $"Failed to delete item with ID {itemRequest.ItemId}. Please verify the ID and try again.");
        }
    }
}