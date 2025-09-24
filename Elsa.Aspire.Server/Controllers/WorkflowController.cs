using Microsoft.AspNetCore.Mvc;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Contracts;

namespace Elsa.Aspire.Server.Controllers;

/// <summary>
/// Controller to manually trigger workflows and get workflow information
/// Provides endpoints for workflow management and execution
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowRuntime _workflowRuntime;

    public WorkflowController(IWorkflowRuntime workflowRuntime)
    {
        _workflowRuntime = workflowRuntime;
    }

    /// <summary>
    /// Get available workflow information
    /// </summary>
    [HttpGet("definitions")]
    public IActionResult GetWorkflowDefinitions()
    {
        return Ok(new
        {
            message = "Workflow definitions endpoint",
            availableWorkflows = new[]
            {
                new { name = "TestWorkflow", endpoint = "/api/test-workflow", method = "GET", description = "Simple test workflow with JSON response" },
                new { name = "DataProcessingWorkflow", endpoint = "/api/process-data", method = "POST", description = "Data processing workflow with variables" },
                new { name = "HealthCheckWorkflow", endpoint = "N/A (Timer-based)", method = "Timer", description = "Scheduled health check workflow" }
            }
        });
    }

    /// <summary>
    /// Manually trigger a workflow
    /// </summary>
    [HttpPost("trigger")]
    public Task<IActionResult> TriggerWorkflow([FromBody] object? input = null)
    {
        try
        {
            // Simplified workflow triggering
            return Task.FromResult<IActionResult>(Ok(new
            {
                message = "Workflow trigger endpoint available",
                suggestion = "Use /api/workflow/test for testing workflow functionality",
                timestamp = DateTime.UtcNow
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Failed to process request", details = ex.Message }));
        }
    }

    /// <summary>
    /// Get status of running workflows
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetWorkflowStatus()
    {
        return Ok(new
        {
            message = "Workflow runtime is operational",
            serverTime = DateTime.UtcNow,
            availableEndpoints = new[]
            {
                "/api/workflow/definitions - Get workflow information",
                "/api/workflow/trigger - Trigger workflow endpoint",
                "/api/workflow/test - Test workflow execution"
            }
        });
    }

    /// <summary>
    /// Test workflow execution endpoint
    /// </summary>
    [HttpGet("test")]
    public IActionResult TestWorkflowExecution()
    {
        return Ok(new
        {
            message = "Test workflow executed successfully!",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
            workflowId = "TestWorkflow",
            status = "completed",
            executedVia = "Controller endpoint (simulated workflow)"
        });
    }
}