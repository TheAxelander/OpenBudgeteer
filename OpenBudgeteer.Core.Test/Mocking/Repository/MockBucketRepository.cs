using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBucketRepository : IBucketRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBucketRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<Bucket> All()
    {
        return _mockDatabase.Buckets.Values;
    }

    public IEnumerable<Bucket> AllWithVersions()
    {
        var mockBucketVersionRepository = new MockBucketVersionRepository(_mockDatabase);
        var buckets = All().ToList();
        foreach (var bucket in buckets)
        {
            bucket.BucketVersions = new List<BucketVersion>();
            foreach (var bucketVersion in mockBucketVersionRepository
                         .All()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BucketVersions.Add(bucketVersion);
            }
        }

        return buckets;
    }

    public IEnumerable<Bucket> AllWithActivities()
    {
        var mockBucketMovementRepository = new MockBucketMovementRepository(_mockDatabase);
        var mockBudgetedTransactionRepository = new MockBudgetedTransactionRepository(_mockDatabase);
        var buckets = All().ToList();
        foreach (var bucket in buckets)
        {
            bucket.BucketMovements = new List<BucketMovement>();
            foreach (var bucketMovement in mockBucketMovementRepository
                         .All()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BucketMovements.Add(bucketMovement);
            }

            bucket.BudgetedTransactions = new List<BudgetedTransaction>();
            foreach (var budgetedTransaction in mockBudgetedTransactionRepository
                         .AllWithTransactions()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BudgetedTransactions.Add(budgetedTransaction);
            }
        }

        return buckets;
    }

    public IEnumerable<Bucket> AllWithIncludedEntities()
    {
        var mockBucketGroupRepository = new MockBucketGroupRepository(_mockDatabase);
        var mockBucketMovementRepository = new MockBucketMovementRepository(_mockDatabase);
        var mockBucketVersionRepository = new MockBucketVersionRepository(_mockDatabase);
        var mockBudgetedTransactionRepository = new MockBudgetedTransactionRepository(_mockDatabase);
        var buckets = All().ToList();
        foreach (var bucket in buckets)
        {
            bucket.BucketGroup = mockBucketGroupRepository.ById(bucket.BucketGroupId) 
                                 ?? throw new Exception("BucketGroup doesn't exist");

            bucket.BucketMovements = new List<BucketMovement>();
            foreach (var bucketMovement in mockBucketMovementRepository
                         .All()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BucketMovements.Add(bucketMovement);
            }

            bucket.BucketVersions = new List<BucketVersion>();
            foreach (var bucketVersion in mockBucketVersionRepository
                         .All()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BucketVersions.Add(bucketVersion);
            }
            
            bucket.BudgetedTransactions = new List<BudgetedTransaction>();
            foreach (var budgetedTransaction in mockBudgetedTransactionRepository
                         .All()
                         .Where(i => i.BucketId == bucket.Id))
            {
                bucket.BudgetedTransactions.Add(budgetedTransaction);
            }
        }

        return buckets;
    }

    public Bucket? ById(Guid id)
    {
        _mockDatabase.Buckets.TryGetValue(id, out var result);
        return result;
    }

    public Bucket? ByIdWithVersions(Guid id)
    {
        var mockBucketVersionRepository = new MockBucketVersionRepository(_mockDatabase);
        var bucket = ById(id);
        if (bucket == null) return bucket;
        bucket.BucketVersions = new List<BucketVersion>();
        foreach (var bucketVersion in mockBucketVersionRepository
                     .All()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BucketVersions.Add(bucketVersion);
        }

        return bucket;
    }

    public Bucket? ByIdWithMovements(Guid id)
    {
        var mockBucketMovementRepository = new MockBucketMovementRepository(_mockDatabase);
        var bucket = ById(id);
        if (bucket == null) return bucket;

        bucket.BucketMovements = new List<BucketMovement>();
        foreach (var bucketMovement in mockBucketMovementRepository
                     .All()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BucketMovements.Add(bucketMovement);
        }

        return bucket;
    }

    public Bucket? ByIdWithTransactions(Guid id)
    {
        var mockBudgetedTransactionRepository = new MockBudgetedTransactionRepository(_mockDatabase);
        var bucket = ById(id);
        if (bucket == null) return bucket;

        bucket.BudgetedTransactions = new List<BudgetedTransaction>();
        foreach (var budgetedTransaction in mockBudgetedTransactionRepository
                     .AllWithTransactions()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BudgetedTransactions.Add(budgetedTransaction);
        }

        return bucket;
    }

    public Bucket? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketGroupRepository = new MockBucketGroupRepository(_mockDatabase);
        var mockBucketMovementRepository = new MockBucketMovementRepository(_mockDatabase);
        var mockBucketVersionRepository = new MockBucketVersionRepository(_mockDatabase);
        var mockBudgetedTransactionRepository = new MockBudgetedTransactionRepository(_mockDatabase);
        var bucket = ById(id);
        if (bucket == null) return bucket;
        bucket.BucketGroup = mockBucketGroupRepository.ById(bucket.BucketGroupId) 
                             ?? throw new Exception("BucketGroup doesn't exist");

        bucket.BucketMovements = new List<BucketMovement>();
        foreach (var bucketMovement in mockBucketMovementRepository
                     .All()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BucketMovements.Add(bucketMovement);
        }

        bucket.BucketVersions = new List<BucketVersion>();
        foreach (var bucketVersion in mockBucketVersionRepository
                     .All()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BucketVersions.Add(bucketVersion);
        }
            
        bucket.BudgetedTransactions = new List<BudgetedTransaction>();
        foreach (var budgetedTransaction in mockBudgetedTransactionRepository
                     .All()
                     .Where(i => i.BucketId == bucket.Id))
        {
            bucket.BudgetedTransactions.Add(budgetedTransaction);
        }

        return bucket;
    }

    public int Create(Bucket entity)
    {
        entity.Id = Guid.NewGuid();
        var version = entity.BucketVersions!.First();
        version.Id = Guid.NewGuid();
        version.BucketId = entity.Id;
        _mockDatabase.Buckets[entity.Id] = entity;
        _mockDatabase.BucketVersions[version.Id] = version;
        return 2;
    }

    public int CreateRange(IEnumerable<Bucket> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(Bucket entity)
    {
        try
        {
            var result = 1;
            _mockDatabase.Buckets[entity.Id] = entity;
            if (entity.BucketVersions == null) return result;
            foreach (var bucketVersion in entity.BucketVersions)
            {
                if (bucketVersion.Id == Guid.Empty)
                {
                    bucketVersion.Id = Guid.NewGuid();
                    result++;
                }
                _mockDatabase.BucketVersions[bucketVersion.Id] = bucketVersion;
            }
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<Bucket> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            var result = _mockDatabase.BucketVersions
                .Where(i => i.Value.BucketId == id)
                .Select(i => i.Key)
                .Count(bucketVersionId => _mockDatabase.BucketVersions.Remove(bucketVersionId));
            if (_mockDatabase.Buckets.Remove(id)) result++;
            
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        return ids.Sum(Delete);
    }
}