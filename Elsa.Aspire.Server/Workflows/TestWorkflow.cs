using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace Elsa.Aspire.Server.Workflows;

public class TestWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        // Create a simple HTTP endpoint that responds with JSON
        var httpEndpoint = new HttpEndpoint();
        var writeResponse = new WriteHttpResponse();

        builder.Root = httpEndpoint;
        builder.Add(writeResponse);
    }
}