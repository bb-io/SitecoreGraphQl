using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl.DataHandlerTests;

[TestClass]
public class LanguageDataSourceTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new LanguageDataSource(InvocationContext);

    protected override string SearchString => "English";
}