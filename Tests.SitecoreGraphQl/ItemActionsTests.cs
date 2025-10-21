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
}
