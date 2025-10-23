using Apps.SitecoreGraphQl.Actions;
using Apps.SitecoreGraphQl.Models.Requests;
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
    
    [TestMethod]
    public async Task UpdateWorkflowState_ValidIds_Success()
    {
        var workflowActions = new WorkflowActions(InvocationContext);
        
        var request = new UpdateWorkflowStateRequest
        {
            WorkflowCommandId = "{F744CC9C-4BB1-4B38-8D5C-1E9CE7F45D2D}",
            ContentId = "{A6D76C0C-5CC9-4AE1-BD63-E3B6DADAAFA8}"
        };
        
        var result = await workflowActions.UpdateWorkflowState(request);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Successful);
        
        PrintObject(result);
    }
}
