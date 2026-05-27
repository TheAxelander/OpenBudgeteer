namespace OpenBudgeteer.Core.Data.Contracts.Services;

/// <summary>
/// Provides methods for exporting budget data to CSV format.
/// All methods return UTF-8 encoded byte arrays suitable for direct file download.
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports bank transactions as CSV. Split transactions produce multiple rows (one per bucket assignment).
    /// Unassigned transactions produce one row with empty bucket columns.
    /// </summary>
    /// <param name="startDate">Inclusive start date filter. Null means no lower bound.</param>
    /// <param name="endDate">Inclusive end date filter. Null means no upper bound.</param>
    /// <param name="accountId">Optional account filter. Null exports all accounts.</param>
    /// <returns>UTF-8 CSV bytes with columns: date, payee, memo, amount, type, account, bucket, bucket_group, bucket_amount</returns>
    byte[] ExportTransactions(DateOnly? startDate, DateOnly? endDate, Guid? accountId = null);

    /// <summary>
    /// Exports all buckets (excluding internal system buckets) as CSV.
    /// Uses the most recent BucketVersion for type and target information.
    /// </summary>
    /// <param name="includeInactive">When false (default), only active buckets are exported.</param>
    /// <returns>UTF-8 CSV bytes with columns: bucket_id, name, group, bucket_type, target_amount, notes, valid_from, is_active</returns>
    byte[] ExportBuckets(bool includeInactive = false);

    /// <summary>
    /// Exports bucket movements (manual fund transfers between buckets) as CSV.
    /// </summary>
    /// <param name="startDate">Inclusive start date filter. Null means no lower bound.</param>
    /// <param name="endDate">Inclusive end date filter. Null means no upper bound.</param>
    /// <returns>UTF-8 CSV bytes with columns: movement_id, movement_date, bucket, amount</returns>
    byte[] ExportBucketMovements(DateOnly? startDate, DateOnly? endDate);
}
