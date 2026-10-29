using DotnetSecurityFailures.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Controller demonstrating Missing Rate Limiting vulnerability
/// 
/// This controller contains INTENTIONALLY VULNERABLE code to demonstrate
/// how lack of rate limiting allows brute force attacks.
/// 
/// Used by: /vulnerabilities/missing-rate-limiting
/// </summary>
[ApiController]
[Route("api/vulnerabilities/missing-rate-limiting")]
public class MissingRateLimitingController : VulnerabilityDemoControllerBase
{
    private const string CorrectPin = "7384";
    private readonly LoginAttemptAllowanceService _loginAttemptAllowanceService;

    public MissingRateLimitingController(
        ILogger<MissingRateLimitingController> logger,
        LoginAttemptAllowanceService loginAttemptAllowanceService)
        : base(logger, "missing-rate-limiting")
    {
        _loginAttemptAllowanceService = loginAttemptAllowanceService;
    }

    // VULNERABLE: No rate limiting on authentication endpoint
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        LogDemoActivity("Login", $"Login attempt with PIN: {request.Pin}");

        if (!_loginAttemptAllowanceService.TryConsumeAttempt("pin-login"))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new LoginResponse
            {
                Success = false,
                Message = "Too many login attempts. Please try again later."
            });
        }
        
        if (string.IsNullOrWhiteSpace(request.Pin))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "PIN is required"
            });
        }

        if (request.Pin == CorrectPin)
        {
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                SecretData = "Confidential account data: Balance=$50,000, Account#1234567890"
            });
        }

        return Unauthorized(new LoginResponse
        {
            Success = false,
            Message = "Invalid PIN"
        });
    }

    public class LoginRequest
    {
        public string Pin { get; set; } = "";
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? SecretData { get; set; }
    }
}
