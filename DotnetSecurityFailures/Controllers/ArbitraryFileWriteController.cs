using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Controller demonstrating Arbitrary File Write vulnerability
/// 
/// This controller contains INTENTIONALLY VULNERABLE code to demonstrate
/// how arbitrary file write attacks can write to any location on the filesystem.
/// 
/// Used by: /vulnerabilities/arbitrary-file-write
/// </summary>
[ApiController]
[Route("api/vulnerabilities/arbitrary-file-write")]
public class ArbitraryFileWriteController : VulnerabilityDemoControllerBase
{
    private readonly string _baseDirectory;

    public ArbitraryFileWriteController(
        ILogger<ArbitraryFileWriteController> logger,
        IWebHostEnvironment env) 
        : base(logger, "arbitrary-file-write")
    {
        _baseDirectory = Path.Combine(env.ContentRootPath, "Backups");
        Directory.CreateDirectory(_baseDirectory);
    }

    [HttpPost("save")]
    public IActionResult SaveBackup([FromBody] BackupRequest request)
    {
        LogDemoActivity("SaveBackup", $"Attempting to write file: {request.FileName}");
        
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return BadRequest(new { success = false, message = "Filename is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { success = false, message = "Content is required" });
        }

        try
        {
            var baseDirectoryPath = Path.GetFullPath(_baseDirectory);
            if (!baseDirectoryPath.EndsWith(Path.DirectorySeparatorChar))
            {
                baseDirectoryPath += Path.DirectorySeparatorChar;
            }

            var actualPath = Path.GetFullPath(Path.Combine(baseDirectoryPath, request.FileName));
            if (!actualPath.StartsWith(baseDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "Invalid filename", fileName = request.FileName });
            }

            System.IO.File.WriteAllText(actualPath, request.Content);

            return Ok(new
            {
                success = true,
                fileName = request.FileName,
                actualPath = actualPath,
                isVulnerable = false,
                message = "Backup saved successfully"
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                success = false,
                message = $"Error: {ex.Message}",
                fileName = request.FileName
            });
        }
    }

    public class BackupRequest
    {
        public string FileName { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
