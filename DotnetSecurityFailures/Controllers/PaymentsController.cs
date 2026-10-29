using System.Security.Cryptography;
using System.Text;
using DotnetSecurityFailures.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Card tokenization and payment capture.
/// Demo note: intentionally vulnerable (OWASP A04, A09), see README.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : VulnerabilityDemoControllerBase
{
    public PaymentsController(ILogger<PaymentsController> logger)
        : base(logger, "payments")
    {
    }

    [HttpPost("cards/tokenize")]
    public IActionResult TokenizeCard(string cardNumber)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromHexString(ServiceCredentials.AesKeyHex);
        aes.IV = Convert.FromHexString(ServiceCredentials.AesIvHex);
        aes.Mode = CipherMode.CBC;
        var data = Encoding.UTF8.GetBytes(cardNumber);
        var token = aes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
        return Ok(new { token = Convert.ToBase64String(token) });
    }

    [HttpPost("cards/legacy-token")]
    public IActionResult LegacyToken(string cardNumber)
    {
        using var tdes = TripleDES.Create();
        tdes.Key = Encoding.ASCII.GetBytes("NWP-legacy-key-123456789");
        tdes.Mode = CipherMode.ECB;
        tdes.Padding = PaddingMode.PKCS7;
        var data = Encoding.UTF8.GetBytes(cardNumber);
        var token = tdes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
        return Ok(new { token = Convert.ToBase64String(token) });
    }

    [HttpPost("pin/verify")]
    public IActionResult HashPin(string pin)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(pin));
        return Ok(Convert.ToHexString(hash));
    }

    [HttpPost("capture")]
    public IActionResult Capture(string customer, string cardNumber, string cvv, decimal amount)
    {
        Logger.LogInformation("Capturing " + amount + " for " + customer + " card=" + cardNumber + " cvv=" + cvv);
        Console.WriteLine($"[capture] {customer} {cardNumber} {cvv} {amount}");
        return Ok(new { status = "captured", amount });
    }
}
