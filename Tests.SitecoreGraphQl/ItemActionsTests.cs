using Apps.SitecoreGraphQl.Actions;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl;

[TestClass]
public class ItemActionsTests : TestBase
{
    [TestMethod]
    public async Task GetItem_ValidId_Success()
    {
        var itemActions = new ItemActions(InvocationContext);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ItemRequest
        {
            ItemId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}"
        };
        
        var result = await itemActions.GetItem(itemRequest);
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ItemId);
        PrintObject(result);
    }

    [TestMethod]
    public async Task GetItem_NotExistingId_ThrowApplicationException()
    {
        var itemActions = new ItemActions(InvocationContext);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ItemRequest
        {
            ItemId = "{00000000-0000-0000-0000-000000000000}"
        };

        var exception = await Assert.ThrowsExceptionAsync<Blackbird.Applications.Sdk.Common.Exceptions.PluginApplicationException>(async () =>
        {
            await itemActions.GetItem(itemRequest);
        });
        
        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Message);
        
        PrintObject(new {message = exception.Message});
    }
}
