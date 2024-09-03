using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBucketGroupRepository : IBucketGroupRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBucketGroupRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<BucketGroup> All()
    {
        return _mockDatabase.BucketGroups.Values;
    }

    public IEnumerable<BucketGroup> AllWithIncludedEntities()
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketGroups = All().ToList();
        foreach (var bucketGroup in bucketGroups)
        {
            bucketGroup.Buckets = new List<Bucket>();
            foreach (var bucket in mockBucketRepository
                         .All()
                         .Where(i => i.BucketGroupId == bucketGroup.Id))
            {
                bucketGroup.Buckets.Add(bucket);
            }
        }

        return bucketGroups;
    }

    public BucketGroup? ById(Guid id)
    {
        _mockDatabase.BucketGroups.TryGetValue(id, out var result);
        return result;
    }

    public BucketGroup? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketGroup = ById(id);
        if (bucketGroup == null) return bucketGroup;
        bucketGroup.Buckets = new List<Bucket>();
        foreach (var bucket in mockBucketRepository
                     .All()
                     .Where(i => i.BucketGroupId == bucketGroup.Id))
        {
            bucketGroup.Buckets.Add(bucket);
        }

        return bucketGroup;
    }

    public int Create(BucketGroup entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.BucketGroups[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<BucketGroup> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketGroup entity)
    {
        try
        {
            _mockDatabase.BucketGroups[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<BucketGroup> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.BucketGroups.Remove(id) ? 1 : 0;
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