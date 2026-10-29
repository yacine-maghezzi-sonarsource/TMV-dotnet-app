using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetSecurityFailures.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Token issuance and introspection for partner integrations.
/// Demo note: intentionally vulnerable (OWASP A04, A07), see README.
/// Uses System.IdentityModel.Tokens.Jwt 6.32.0 (CVE-2024-21319).
/// </summary>
[ApiController]
[Route("api/partners/auth")]
public class PartnerAuthController : VulnerabilityDemoControllerBase
{
    public PartnerAuthController(ILogger<PartnerAuthController> logger)
        : base(logger, "partner-auth")
    {
    }

    [HttpPost("token")]
    public IActionResult IssueToken(string partnerId, string role = "partner")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ServiceCredentials.JwtSigningKey));
        var token = new JwtSecurityToken(
            issuer: "https://auth.northwindpay.internal",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, partnerId),
                new Claim(ClaimTypes.Role, role)
            },
            expires: DateTime.UtcNow.AddYears(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("introspect")]
    public IActionResult Introspect([FromForm] string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false,
            RequireExpirationTime = false,
            SignatureValidator = (jwt, _) => new JwtSecurityToken(jwt)
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
        return Ok(new
        {
            active = true,
            sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            role = principal.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}
