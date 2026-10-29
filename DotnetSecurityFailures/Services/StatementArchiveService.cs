using Ionic.Zip;

namespace DotnetSecurityFailures.Services;

/// <summary>
/// Stores and unpacks bank statement bundles uploaded by business customers.
///
/// Demo note (see README): intentionally vulnerable. Saves uploads under the
/// client supplied file name inside the web root (CWE-22, CWE-434) and extracts
/// archives with DotNetZip 1.16.0 ZipFile.ExtractAll (CVE-2024-48510, zip slip).
/// </summary>
public class StatementArchiveService
{
    private readonly string _uploadRoot;

    public StatementArchiveService(IWebHostEnvironment environment)
    {
        _uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "statements");
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        var path = Path.Combine(_uploadRoot, file.FileName);
        await using var output = File.Create(path);
        await file.CopyToAsync(output);
        return "/uploads/statements/" + file.FileName;
    }

    public IReadOnlyList<string> Unpack(string customerId, Stream archive)
    {
        var destination = Path.Combine(_uploadRoot, customerId);
        Directory.CreateDirectory(destination);

        using (var zip = ZipFile.Read(archive))
        {
            zip.ExtractAll(destination, ExtractExistingFileAction.OverwriteSilently);
        }

        return Directory.GetFiles(destination, "*", SearchOption.AllDirectories);
    }
}
