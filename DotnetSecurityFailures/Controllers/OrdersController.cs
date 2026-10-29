using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Order search and reporting.
/// Demo note: intentionally vulnerable (OWASP A05), see README.
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : VulnerabilityDemoControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public OrdersController(ILogger<OrdersController> logger, IWebHostEnvironment environment)
        : base(logger, "orders")
    {
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Search(string customer, string sort = "created_at")
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM Orders WHERE customer = '{customer}' ORDER BY {sort}";
        try
        {
            using var reader = command.ExecuteReader();
            return Ok(reader.HasRows);
        }
        catch (Exception ex)
        {
            return Ok(new { query = command.CommandText, error = ex.ToString() });
        }
    }

    [HttpGet("match")]
    public IActionResult MatchReference(string pattern, string reference)
    {
        return Ok(Regex.IsMatch(reference, pattern));
    }

    [HttpGet("reports/{name}")]
    public IActionResult DownloadReport(string name)
    {
        var path = Path.Combine(_environment.ContentRootPath, "Documents", name);
        return Ok(System.IO.File.ReadAllText(path));
    }
}
