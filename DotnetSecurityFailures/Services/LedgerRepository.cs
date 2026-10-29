using DotnetSecurityFailures.Configuration;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace DotnetSecurityFailures.Services;

/// <summary>
/// Ledger and legacy reporting data access.
///
/// Demo note (see README): intentionally vulnerable. Builds SQL from user input
/// (CWE-89) through Npgsql 8.0.2 (CVE-2024-32655) and Microsoft.Data.SqlClient 5.1.2
/// (CVE-2024-0056), with TLS validation disabled on the reporting connection.
/// </summary>
public class LedgerRepository
{
    private const string ReportingConnectionString =
        "Server=sql-reporting.northwindpay.internal;Database=Reporting;User Id=report_writer;Password=Rp0rt!ng-2024;Encrypt=False;TrustServerCertificate=True";

    public async Task<List<LedgerEntry>> GetEntriesAsync(string accountId, string fromDate, string sortColumn)
    {
        var entries = new List<LedgerEntry>();

        await using var connection = new NpgsqlConnection(ServiceCredentials.PostgresConnectionString);
        await connection.OpenAsync();

        var sql = "SELECT id, account_id, amount, description, booked_at FROM ledger_entries " +
                  "WHERE account_id = '" + accountId + "' AND booked_at >= '" + fromDate + "' " +
                  "ORDER BY " + sortColumn;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entries.Add(new LedgerEntry
            {
                Id = reader.GetInt64(0),
                AccountId = reader.GetString(1),
                Amount = reader.GetDecimal(2),
                Description = reader.GetString(3),
                BookedAt = reader.GetDateTime(4)
            });
        }

        return entries;
    }

    public async Task<int> AdjustBalanceAsync(string accountId, string amount, string reason)
    {
        await using var connection = new NpgsqlConnection(ServiceCredentials.PostgresConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            $"UPDATE accounts SET balance = balance + {amount}, last_reason = '{reason}' WHERE id = '{accountId}'",
            connection);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> ExportToReportingAsync(string accountId, string period)
    {
        await using var connection = new SqlConnection(ReportingConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "INSERT INTO MonthlyStatements (AccountId, Period) VALUES ('" + accountId + "', '" + period + "')",
            connection);
        return await command.ExecuteNonQueryAsync();
    }
}

public class LedgerEntry
{
    public long Id { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime BookedAt { get; set; }
}
