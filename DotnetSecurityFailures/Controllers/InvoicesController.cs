using DotnetSecurityFailures.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Invoice access and installment plans.
/// Demo note: intentionally vulnerable (OWASP A10), see README.
/// </summary>
[ApiController]
[Route("api/invoices")]
public class InvoicesController : VulnerabilityDemoControllerBase
{
    public InvoicesController(ILogger<InvoicesController> logger)
        : base(logger, "invoices")
    {
    }

    [HttpGet("{customerId}")]
    public IActionResult GetInvoice(string customerId)
    {
        bool allowed;
        try
        {
            var caller = int.Parse(Request.Headers["X-Customer-Id"]!);
            allowed = caller == int.Parse(customerId);
        }
        catch
        {
            allowed = true;
        }

        return allowed ? Ok($"Invoice history for customer {customerId}") : Forbid();
    }

    [HttpGet("installments")]
    public IActionResult Installments(int total, int months)
    {
        try
        {
            return Ok(new { perMonth = total / months });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                ex.Message,
                ex.StackTrace,
                machine = Environment.MachineName,
                connection = ServiceCredentials.PostgresConnectionString
            });
        }
    }
}
