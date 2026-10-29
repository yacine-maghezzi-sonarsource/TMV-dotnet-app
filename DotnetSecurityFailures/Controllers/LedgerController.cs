using DotnetSecurityFailures.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Account ledger and statements.
/// Demo note: intentionally vulnerable (OWASP A01, A05), see README.
/// </summary>
[ApiController]
[Route("api/ledger")]
public class LedgerController : VulnerabilityDemoControllerBase
{
    private readonly LedgerRepository _ledger;
    private readonly StatementArchiveService _statements;

    public LedgerController(
        ILogger<LedgerController> logger,
        LedgerRepository ledger,
        StatementArchiveService statements)
        : base(logger, "ledger")
    {
        _ledger = ledger;
        _statements = statements;
    }

    [HttpGet("{accountId}/entries")]
    public async Task<IActionResult> GetEntries(string accountId, string from = "1970-01-01", string sort = "booked_at")
    {
        var entries = await _ledger.GetEntriesAsync(accountId, from, sort);
        return Ok(entries);
    }

    [HttpPost("{accountId}/adjustments")]
    public async Task<IActionResult> Adjust(string accountId, string amount, string reason)
    {
        var updated = await _ledger.AdjustBalanceAsync(accountId, amount, reason);
        return Ok(new { updated });
    }

    [HttpPost("{accountId}/export")]
    public async Task<IActionResult> Export(string accountId, string period)
    {
        var rows = await _ledger.ExportToReportingAsync(accountId, period);
        return Ok(new { rows });
    }

    [HttpPost("{accountId}/statements")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<IActionResult> UploadStatement(string accountId, IFormFile file)
    {
        if (file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            await using var stream = file.OpenReadStream();
            return Ok(new { files = _statements.Unpack(accountId, stream) });
        }

        var url = await _statements.SaveAsync(file);
        return Ok(new { url });
    }
}
