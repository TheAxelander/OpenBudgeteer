using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BucketMapperExtensions
{
    public static Bucket MapWithBucketGroup(this Bucket bucket, BucketGroup bucketGroup)
    {
        bucket.BucketGroup = bucketGroup;
        return bucket;
    }

    public static Bucket MapWithBudgetedTransaction(this Bucket bucket, BudgetedTransaction? budgetedTransaction)
    {
        BaseMapperExtensions.AddWithBackReference(
            bucket.BudgetedTransactions ??= new List<BudgetedTransaction>(),
            budgetedTransaction,
            bt => bt.Id,
            bt => bt.Bucket = bucket);
        return bucket;
    }

    public static Bucket MapWithMovement(this Bucket bucket, BucketMovement? movement)
    {
        BaseMapperExtensions.AddWithBackReference(
            bucket.BucketMovements ??= new List<BucketMovement>(),
            movement,
            bm => bm.Id,
            bm => bm.Bucket = bucket);
        return bucket;
    }

    public static Bucket MapWithVersion(this Bucket bucket, BucketVersion? version)
    {
        BaseMapperExtensions.AddWithBackReference(
            bucket.BucketVersions ??= new List<BucketVersion>(),
            version,
            bv => bv.Id,
            bv => bv.Bucket = bucket);
        return bucket;
    }
}

public class BucketMapper
{
    public IEnumerable<Bucket> Results => _cache.Values;

    private readonly Dictionary<Guid, Bucket> _cache = new();

    public Bucket MapWithMovement(Bucket bucket, BucketMovement? movement)
    {
        var result = GetOrAdd(bucket);
        return result.MapWithMovement(movement);
    }

    public Bucket MapWithVersion(Bucket bucket, BucketVersion? version)
    {
        var result = GetOrAdd(bucket);
        return result.MapWithVersion(version);
    }

    public Bucket MapWithActivities(Bucket bucket, BucketMovement? movement, BudgetedTransaction? transaction)
    {
        var result = GetOrAdd(bucket);
        return result
            .MapWithMovement(movement)
            .MapWithBudgetedTransaction(transaction);
    }

    public Bucket MapWithTransactions(
        Bucket bucket,
        BudgetedTransaction? budgetedTransaction,
        BankTransaction? bankTransaction)
    {
        var result = GetOrAdd(bucket);
        if (budgetedTransaction != null && bankTransaction != null)
        {
            budgetedTransaction.Transaction = bankTransaction;
        }
        return result.MapWithBudgetedTransaction(budgetedTransaction);
    }

    public Bucket MapWithEverything(
        Bucket bucket,
        BucketGroup bucketGroup,
        BucketMovement? movement,
        BucketVersion? version,
        BudgetedTransaction? transaction)
    {
        var result = GetOrAdd(bucket);
        return result
            .MapWithBucketGroup(bucketGroup)
            .MapWithMovement(movement)
            .MapWithVersion(version)
            .MapWithBudgetedTransaction(transaction);
    }

    private Bucket GetOrAdd(Bucket bucket)
    {
        if (_cache.TryGetValue(bucket.Id, out var existing))
            return existing;

        existing = bucket;
        _cache.Add(bucket.Id, existing);

        return existing;
    }
}
