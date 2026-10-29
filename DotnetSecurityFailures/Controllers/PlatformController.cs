using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DotnetSecurityFailures.Controllers;

/// <summary>
/// Operations endpoints: diagnostics, plugins and updates.
/// Demo note: intentionally vulnerable (OWASP A02, A03, A04, A05, A08), see README.
/// </summary>
[ApiController]
[Route("api/platform")]
public class PlatformController : VulnerabilityDemoControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PlatformController(ILogger<PlatformController> logger, IHttpClientFactory httpClientFactory)
        : base(logger, "platform")
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("diagnostics/env")]
    public IActionResult EnvironmentSnapshot()
    {
        return Ok(Environment.GetEnvironmentVariables());
    }

    [HttpGet("diagnostics/ping")]
    public IActionResult Ping(string host)
    {
        var psi = new ProcessStartInfo("/bin/sh", "-c \"ping -c 1 " + host + "\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi)!;
        return Ok(process.StandardOutput.ReadToEnd());
    }

    [HttpPost("plugins")]
    public async Task<IActionResult> InstallPlugin(string name)
    {
        var client = _httpClientFactory.CreateClient();
        var bytes = await client.GetByteArrayAsync("http://plugins.northwindpay.internal/" + name);
        var assembly = Assembly.Load(bytes);
        assembly.GetTypes().First().GetMethod("Run")?.Invoke(null, null);
        return Ok(assembly.FullName);
    }

    [HttpPost("update")]
    public async Task<IActionResult> SelfUpdate()
    {
        var client = _httpClientFactory.CreateClient();
        var script = await client.GetStringAsync("http://updates.northwindpay.internal/latest/install.sh");
        const string path = "/tmp/install.sh";
        await System.IO.File.WriteAllTextAsync(path, script);
        Process.Start("/bin/bash", path);
        return Accepted();
    }

    [HttpPost("settings/import")]
    public IActionResult ImportSettings([FromBody] string json)
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var imported = JsonConvert.DeserializeObject(json, settings);
        return Ok(imported?.GetType().FullName);
    }

    [HttpGet("keys/partner")]
    public IActionResult GeneratePartnerKey()
    {
        using var rsa = new RSACryptoServiceProvider(1024);
        return Ok(rsa.ToXmlString(true));
    }
}
