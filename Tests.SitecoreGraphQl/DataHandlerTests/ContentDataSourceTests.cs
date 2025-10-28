using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl.DataHandlerTests;

[TestClass]
public class ContentDataSourceTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new ContentDataSource(InvocationContext, new()
    {
        Language = "en"
    });

    protected override string SearchString => "Data";
}