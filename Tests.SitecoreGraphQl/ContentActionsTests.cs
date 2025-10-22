using Apps.SitecoreGraphQl.Actions;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl;

[TestClass]
public class ContentActionsTests : TestBase
{
    [TestMethod]
    public async Task GetContent_ValidId_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ContentRequest
        {
            ContentId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}"
        };
        
        var result = await itemActions.GetContent(itemRequest);
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ItemId);
        PrintObject(result);
    }
    
    [TestMethod]
    public async Task GetContent_ValidIdLanguageAndVersion_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ContentRequest
        {
            ContentId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}",
            Language = "en",
            Version = 1
        };
        
        var result = await itemActions.GetContent(itemRequest);
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ItemId);
        PrintObject(result);
    }

    [TestMethod]
    public async Task GetContent_NotExistingId_ThrowApplicationException()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ContentRequest
        {
            ContentId = "{00000000-0000-0000-0000-000000000000}"
        };

        var exception = await Assert.ThrowsExceptionAsync<Blackbird.Applications.Sdk.Common.Exceptions.PluginApplicationException>(async () =>
        {
            await itemActions.GetContent(itemRequest);
        });
        
        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Message);
        
        PrintObject(new {message = exception.Message});
    }
    
    [TestMethod]
    public async Task DownloadItemContent_ValidId_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ContentRequest
        {
            ContentId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}"
        };
        
        var result = await itemActions.DownloadItemContent(itemRequest);
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content.Name);
        PrintObject(result);
    }
}
