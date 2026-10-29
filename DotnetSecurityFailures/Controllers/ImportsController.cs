using DotnetSecurityFailures.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Supplier integration endpoints. Demo note: intentionally vulnerable, see
/// <see cref="CatalogImportService"/> for the reachable dependency CVEs.
/// </summary>
[ApiController]
[Route("api/imports")]
public class ImportsController : VulnerabilityDemoControllerBase
{
    private readonly CatalogImportService _imports;

    public ImportsController(ILogger<ImportsController> logger, CatalogImportService imports)
        : base(logger, "dependency-reachability")
    {
        _imports = imports;
    }

    [HttpPost("catalog")]
    [Consumes("application/x-yaml", "text/yaml", "text/plain")]
    public async Task<IActionResult> ImportCatalog()
    {
        using var reader = new StreamReader(Request.Body);
        var yaml = await reader.ReadToEndAsync();
        var catalog = _imports.ImportCatalog(yaml);
        return Ok(new { catalog.Supplier, items = catalog.Items.Count });
    }

    [HttpPost("shipments/{shipmentId}")]
    public IActionResult UploadShipment(string shipmentId, IFormFile archive)
    {
        using var stream = archive.OpenReadStream();
        var files = _imports.ExtractShipment(shipmentId, stream);
        return Ok(new { shipmentId, files });
    }

    [HttpPut("logging-profile")]
    public IActionResult UpdateLoggingProfile()
    {
        _imports.ApplyLoggingProfile(Request.Body);
        return NoContent();
    }
}
