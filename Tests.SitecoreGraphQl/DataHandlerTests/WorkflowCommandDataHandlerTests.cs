using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl.DataHandlerTests;

[TestClass]
public class WorkflowCommandDataHandlerTests : BaseDataHandlerTests
{
    protected override IAsyncDataSourceItemHandler DataHandler => new WorkflowCommandDataHandler(InvocationContext, new()
    {
        ItemId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}"
    });

    protected override string SearchString => "Approve";
}