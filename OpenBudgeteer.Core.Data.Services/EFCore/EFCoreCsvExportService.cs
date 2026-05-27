using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

/// <summary>
/// Exports budget data to RFC 4180-compliant CSV bytes.
/// Unlike other services this class does NOT extend EFCoreBaseService because export
/// is a read-only cross-entity concern that benefits from custom EF Core Include chains
/// rather than going through the single-entity repository layer.
/// </summary>
public class EFCoreCsvExportService : ICsvExportService
{
    // Internal system bucket IDs that should never appear in user-facing exports.
    // 000001 = "Income" system bucket, 000002 = "Transfer" system bucket.
    private static readonly Guid SystemBucketIncomeId  = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid SystemBucketTransferId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreCsvExportService> _logger;

    public EFCoreCsvExportService(
        IDbContextFactory<DatabaseContext> dbContextFactory,
        ILogger<EFCoreCsvExportService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Exports bank transactions. Each BudgetedTransaction (bucket assignment) becomes its own
    /// CSV row so that split transactions are fully visible to analysis tools.
    /// Transactions with no bucket assignment produce one row with empty bucket columns.
    /// </summary>
    public byte[] ExportTransactions(DateOnly? startDate, DateOnly? endDate, Guid? accountId = null)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            // Custom include chain: AllWithIncludedEntities() only includes Account, not
            // BudgetedTransactions or their Buckets, so we query the context directly here.
            var query = dbContext.BankTransaction
                .Include(t => t.Account)
                .Include(t => t.BudgetedTransactions)
                    .ThenInclude(bt => bt.Bucket)
                        .ThenInclude(b => b!.BucketGroup)
                .AsNoTracking()
                .Where(t =>
                    t.TransactionDate >= (startDate ?? DateOnly.MinValue) &&
                    t.TransactionDate <= (endDate   ?? DateOnly.MaxValue));

            if (accountId.HasValue)
                query = query.Where(t => t.AccountId == accountId.Value);

            var transactions = query.OrderBy(t => t.TransactionDate).ToList();

            var headers = new[] { "date", "payee", "memo", "amount", "type", "account", "bucket", "bucket_group", "bucket_amount" };
            var rows = new List<string[]>();

            foreach (var tx in transactions)
            {
                var date    = tx.TransactionDate.ToString("yyyy-MM-dd");
                var payee   = tx.Payee ?? string.Empty;
                var memo    = tx.Memo  ?? string.Empty;
                var amount  = tx.Amount.ToString("G", CultureInfo.InvariantCulture);
                var type    = tx.Amount >= 0 ? "credit" : "debit";
                var account = tx.Account?.Name ?? string.Empty;

                if (tx.BudgetedTransactions == null || !tx.BudgetedTransactions.Any())
                {
                    // Unassigned transaction — one row with empty bucket columns
                    rows.Add(new[] { date, payee, memo, amount, type, account, "", "", "" });
                }
                else
                {
                    // Split transaction — one row per bucket assignment
                    foreach (var bt in tx.BudgetedTransactions)
                    {
                        var bucketName  = bt.Bucket?.Name          ?? string.Empty;
                        var bucketGroup = bt.Bucket?.BucketGroup?.Name ?? string.Empty;
                        var bucketAmt   = bt.Amount.ToString("G", CultureInfo.InvariantCulture);
                        rows.Add(new[] { date, payee, memo, amount, type, account, bucketName, bucketGroup, bucketAmt });
                    }
                }
            }

            return BuildCsvBytes(rows, headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error exporting transactions to CSV.");
            throw;
        }
    }

    /// <summary>
    /// Exports all user-created buckets. System buckets (Income, Transfer) are always excluded.
    /// Uses the most-recent BucketVersion for type and target amount information.
    /// </summary>
    public byte[] ExportBuckets(bool includeInactive = false)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var query = dbContext.Bucket
                .Include(b => b.BucketGroup)
                .Include(b => b.BucketVersions)
                .AsNoTracking()
                .Where(b =>
                    b.Id != SystemBucketIncomeId &&
                    b.Id != SystemBucketTransferId);

            if (!includeInactive)
                query = query.Where(b => !b.IsInactive);

            var buckets = query.OrderBy(b => b.BucketGroup!.Position).ThenBy(b => b.Name).ToList();

            var headers = new[] { "bucket_id", "name", "group", "bucket_type", "target_amount", "notes", "valid_from", "is_active" };
            var rows = new List<string[]>();

            foreach (var bucket in buckets)
            {
                // Select the most recent version as the "current" configuration
                var version = bucket.BucketVersions?
                    .OrderByDescending(v => v.ValidFrom)
                    .FirstOrDefault();

                rows.Add(new[]
                {
                    bucket.Id.ToString(),
                    bucket.Name          ?? string.Empty,
                    bucket.BucketGroup?.Name ?? string.Empty,
                    version != null ? BucketTypeToString(version.BucketType) : string.Empty,
                    version?.BucketTypeYParam.ToString("G", CultureInfo.InvariantCulture) ?? "0",
                    version?.Notes       ?? string.Empty,
                    bucket.ValidFrom.ToString("yyyy-MM-dd"),
                    (!bucket.IsInactive).ToString().ToLowerInvariant()
                });
            }

            return BuildCsvBytes(rows, headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error exporting buckets to CSV.");
            throw;
        }
    }

    /// <summary>
    /// Exports bucket movements (manual fund transfers between buckets) ordered by date.
    /// </summary>
    public byte[] ExportBucketMovements(DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var movements = dbContext.BucketMovement
                .Include(m => m.Bucket)
                .AsNoTracking()
                .Where(m =>
                    m.MovementDate >= (startDate ?? DateOnly.MinValue) &&
                    m.MovementDate <= (endDate   ?? DateOnly.MaxValue))
                .OrderBy(m => m.MovementDate)
                .ToList();

            var headers = new[] { "movement_id", "movement_date", "bucket", "amount" };
            var rows = movements.Select(m => new[]
            {
                m.Id.ToString(),
                m.MovementDate.ToString("yyyy-MM-dd"),
                m.Bucket?.Name ?? string.Empty,
                m.Amount.ToString("G", CultureInfo.InvariantCulture)
            }).ToList();

            return BuildCsvBytes(rows, headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error exporting bucket movements to CSV.");
            throw;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    // UTF-8 encoding with BOM (0xEF 0xBB 0xBF) so Excel opens the file correctly
    // without showing garbled characters for non-ASCII payee or bucket names.
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <summary>
    /// Builds a UTF-8 CSV byte array (with BOM for Excel compatibility) from a header row and data rows.
    /// </summary>
    private static byte[] BuildCsvBytes(IEnumerable<string[]> rows, string[] headers)
    {
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", headers.Select(CsvEscape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(CsvEscape)));

        return Utf8WithBom.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Wraps a CSV field in double-quotes if it contains a comma, double-quote, or newline.
    /// Inner double-quotes are escaped by doubling them (RFC 4180 §2.7).
    /// </summary>
    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // Only quote when necessary to keep output clean
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

    /// <summary>
    /// Converts the numeric BucketType identifier to a human-readable label.
    /// Type codes are defined by OpenBudgeteer convention (not an enum in the data layer).
    /// </summary>
    private static string BucketTypeToString(int bucketType) => bucketType switch
    {
        1 => "Standard",
        2 => "Monthly Expense",
        3 => "Expense Every X Months",
        4 => "Monthly Income",
        _ => $"Unknown ({bucketType})"
    };
}
