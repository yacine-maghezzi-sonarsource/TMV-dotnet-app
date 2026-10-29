using System.Security.Cryptography;
using System.Text;
using DotnetSecurityFailures.Configuration;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Payment provider webhooks and merchant callback registration.
/// Demo note: intentionally vulnerable (OWASP A01 SSRF, A08, A09), see README.
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : VulnerabilityDemoControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhooksController(ILogger<WebhooksController> logger, IHttpClientFactory httpClientFactory)
        : base(logger, "webhooks")
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("provider")]
    public async Task<IActionResult> ReceiveProviderEvent()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        Logger.LogInformation("Webhook received: " + body);

        var signature = Request.Headers["X-Signature"].ToString();
        if (!string.IsNullOrEmpty(signature))
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(ServiceCredentials.StripeSecretKey));
            var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
            if (expected != signature)
            {
                return Unauthorized("Bad signature, expected " + expected);
            }
        }

        var evt = JsonConvert.DeserializeObject(body, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        return Ok(new { received = evt?.GetType().Name });
    }

    [HttpPost("merchants/{merchantId}/callback")]
    public async Task<IActionResult> RegisterCallback(string merchantId, string callbackUrl)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync(callbackUrl,
            new StringContent($"{{\"merchant\":\"{merchantId}\",\"event\":\"callback.verified\"}}",
                Encoding.UTF8, "application/json"));

        return Ok(new
        {
            status = (int)response.StatusCode,
            body = await response.Content.ReadAsStringAsync()
        });
    }
}
