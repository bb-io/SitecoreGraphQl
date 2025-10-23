using Apps.SitecoreGraphQl.Actions;
using Tests.SitecoreGraphQl.Base;

namespace Tests.SitecoreGraphQl;

[TestClass]
public class WorkflowActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchWorkflows_Success()
    {
        var workflowActions = new WorkflowActions(InvocationContext);
        
        var result = await workflowActions.SearchWorkflows();
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Workflows);
        Assert.IsTrue(result.Workflows.Count > 0);
        
        PrintObject(result);
    }
}
