using System.Reflection;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using log4net;
using log4net.Config;
using YamlDotNet.Serialization;

namespace DotnetSecurityFailures.Services;

/// <summary>
/// Imports supplier catalogs, shipment archives and logging profiles.
///
/// Demo note (see README): this service is intentionally vulnerable. It calls the
/// vulnerable entry points of three directly referenced packages so that SCA
/// reachability analysis reports their CVEs as reachable:
///   - YamlDotNet 4.3.2       Deserializer.Deserialize          CVE-2018-1000210
///   - SharpZipLib 1.3.2      TarArchive.ExtractContents        CVE-2021-32840
///   - log4net 2.0.9          XmlConfigurator.Configure         CVE-2018-1285
/// </summary>
public class CatalogImportService
{
    private readonly string _importRoot;
    private readonly ILogger<CatalogImportService> _logger;

    public CatalogImportService(IWebHostEnvironment environment, ILogger<CatalogImportService> logger)
    {
        _importRoot = Path.Combine(environment.ContentRootPath, "imports");
        _logger = logger;
        Directory.CreateDirectory(_importRoot);
    }

    public SupplierCatalog ImportCatalog(string yaml)
    {
        var deserializer = new DeserializerBuilder().Build();
        using var reader = new StringReader(yaml);
        var catalog = deserializer.Deserialize<SupplierCatalog>(reader);

        _logger.LogInformation("Imported catalog {Supplier} with {Count} items",
            catalog.Supplier, catalog.Items.Count);
        return catalog;
    }

    public IReadOnlyList<string> ExtractShipment(string shipmentId, Stream tarStream)
    {
        var destination = Path.Combine(_importRoot, "shipments", shipmentId);
        Directory.CreateDirectory(destination);

        using (var archive = TarArchive.CreateInputTarArchive(tarStream, Encoding.UTF8))
        {
            archive.ExtractContents(destination);
        }

        return Directory.GetFiles(destination, "*", SearchOption.AllDirectories);
    }

    public void ApplyLoggingProfile(Stream log4netXml)
    {
        var repository = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
        XmlConfigurator.Configure(repository, log4netXml);
    }
}

public class SupplierCatalog
{
    public string Supplier { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
    public List<CatalogItem> Items { get; set; } = new();
}

public class CatalogItem
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
