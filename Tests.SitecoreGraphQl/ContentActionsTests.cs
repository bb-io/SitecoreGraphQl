using Apps.SitecoreGraphQl.Actions;
using Apps.SitecoreGraphQl.Models.Requests;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl;

[TestClass]
public class ContentActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchContent_EnLanguage_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var searchRequest = new SearchContentRequest
        {
            Language = "en"
        };
        
        var result = await itemActions.SearchContent(searchRequest, new());
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count() > 0);
        
        Console.WriteLine($"Total items: {result.Items.Count()}");
        PrintObject(result);
    }

    [TestMethod]
    public async Task SearchContent_WithRootPath_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var searchRequest = new SearchContentRequest
        {
            RootPath = "/sitecore/content/bb/blackbird/home/partners/data"
        };
        
        var result = await itemActions.SearchContent(searchRequest, new());
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count() > 0);
        
        Console.WriteLine($"Total items: {result.Items.Count()}");
        PrintObject(result);
    }

    [TestMethod]
    public async Task SearchContent_WithDateFilters_Success()
    {
        
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var searchRequest = new SearchContentRequest()
        {
            RootPath = "/sitecore/content/bb/blackbird/home/partners/data"
        };
        var dateFilters = new DateFilters
        {
            CreatedAfter = DateTime.Now.AddMonths(-3)
        };
        
        var result = await itemActions.SearchContent(searchRequest, dateFilters);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count() > 0);
        
        Console.WriteLine($"Total items: {result.Items.Count()}");
        PrintObject(result);
    }
    
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
        Assert.IsNotNull(result.Id);
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
        Assert.IsNotNull(result.Id);
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
            ContentId = "{29B8EF39-F216-4873-8A54-AB5E55E0FDE4}",
            Language = "en"
        };

        var result = await itemActions.DownloadItemContent(itemRequest, new()
        {
            IncludeChildItems = true,
        }, new()
        {
            ExcludeFieldNames = new[] { "IsVerifiedStyle", "Value", "Allowed Renderings", "GenerateSitemapMediaItems", "ThumbnailsRootPath", "RenderingHost", "GenerateThumbnails", "AdditionalChildren", "POS" }
        });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content.Name);
        PrintObject(result);
    }

    [TestMethod]
    public async Task DownloadItemContent_WithExcludeFields_ExcludedFieldsAbsentFromHtml()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var itemRequest = new Apps.SitecoreGraphQl.Models.Requests.ContentRequest
        {
            ContentId = "{29B8EF39-F216-4873-8A54-AB5E55E0FDE4}",
            Language = "en"
        };

        var excludedFields = new[] { "IsVerifiedStyle", "Value", "Allowed Renderings" };

        var result = await itemActions.DownloadItemContent(itemRequest, new()
        {
            IncludeChildItems = true,
        }, new()
        {
            ExcludeFieldNames = excludedFields
        });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content.Name);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectDir = Directory.GetParent(baseDir)!.Parent!.Parent!.Parent!.FullName;
        var outputPath = Path.Combine(projectDir, "TestFiles", "Output", result.Content.Name);
        Assert.IsTrue(File.Exists(outputPath), $"Output file not found: {outputPath}");

        var html = await File.ReadAllTextAsync(outputPath);

        foreach (var field in excludedFields)
        {
            Assert.IsFalse(
                html.Contains($"data-field-name=\"{field}\"", StringComparison.OrdinalIgnoreCase),
                $"Excluded field '{field}' should not appear in HTML output.");
        }

        Console.WriteLine($"Excluded fields verified absent: {string.Join(", ", excludedFields)}");
        PrintObject(result);
    }
    
    [TestMethod]
    public async Task UploadItemContent_ValidFile_Success()
    {
        var itemActions = new ContentActions(InvocationContext, FileManager);
        var uploadContentRequest = new UploadContentRequest
        {
            Content = new()
            {
                Name = "my-test-site.html",
                ContentType = "text/html"
            },
            Locale = "nl-nl"
        };
        
        await itemActions.UploadItemContent(uploadContentRequest);
    }
}
