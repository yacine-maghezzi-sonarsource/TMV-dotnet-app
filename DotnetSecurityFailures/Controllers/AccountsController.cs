using System.Text;
using DotnetSecurityFailures.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Customer account management.
/// Demo note: intentionally vulnerable (OWASP A01, A02, A06, A07), see README.
/// </summary>
[ApiController]
[Route("api/accounts")]
public class AccountsController : VulnerabilityDemoControllerBase
{
    private const string BackofficePassword = "Adm1n!2024-Pr0d";

    private static readonly Dictionary<string, string> PendingResets = new();

    public AccountsController(ILogger<AccountsController> logger)
        : base(logger, "accounts")
    {
    }

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        if (username == "backoffice" && password == BackofficePassword)
        {
            Response.Cookies.Append("nw_session",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:admin")),
                new CookieOptions
                {
                    HttpOnly = false,
                    Secure = false,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddYears(10)
                });
            return Ok(new { token = ServiceCredentials.JwtSigningKey });
        }

        return Unauthorized($"Invalid password for user {username}");
    }

    [HttpGet("logout")]
    public IActionResult Logout(string returnUrl)
    {
        Response.Cookies.Delete("nw_session");
        return Redirect(returnUrl);
    }

    [HttpGet]
    public IActionResult ListAccounts([FromQuery] bool includeInternal)
    {
        if (includeInternal || Request.Cookies["role"] == "admin")
        {
            return Ok(new[]
            {
                new { id = 1, user = "backoffice", ssn = "078-05-1120", salary = 185000 },
                new { id = 2, user = "jdoe", ssn = "219-09-9999", salary = 72000 }
            });
        }

        return Forbid();
    }

    [HttpPost("password/forgot")]
    public IActionResult ForgotPassword(string email)
    {
        var random = new Random(DateTime.Now.Second);
        var code = random.Next(100000, 999999).ToString();
        PendingResets[email] = code;
        return Ok(new { message = "Reset email sent", code });
    }

    [HttpPost("password/reset")]
    public IActionResult ResetPassword(string email, string code, string newPassword)
    {
        if (PendingResets.TryGetValue(email, out var expected) && expected == code)
        {
            return Ok($"Password for {email} changed to {newPassword}");
        }

        return BadRequest();
    }
}
