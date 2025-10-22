using Apps.SitecoreGraphQl.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SitecoreGraphQl.Models.Requests;

public class UpdateWorkflowStateRequest : ContentRequest
{
    [Display("Workflow command ID"), DataSource(typeof(WorkflowCommandDataHandler))]
    public string WorkflowCommandId { get; set; } = string.Empty;
}