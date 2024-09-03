using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBucketVersionRepository : IBucketVersionRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBucketVersionRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<BucketVersion> All()
    {
        return _mockDatabase.BucketVersions.Values;
    }

    public IEnumerable<BucketVersion> AllWithIncludedEntities()
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketVersions = All().ToList();
        foreach (var bucketVersion in bucketVersions)
        {
            bucketVersion.Bucket = mockBucketRepository.ById(bucketVersion.BucketId)
                                   ?? throw new Exception("Bucket doesn't exist");
        }

        return bucketVersions;
    }

    public BucketVersion? ById(Guid id)
    {
        _mockDatabase.BucketVersions.TryGetValue(id, out var result);
        return result;
    }

    public BucketVersion? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketVersion = ById(id);
        if (bucketVersion == null) return bucketVersion;
        bucketVersion.Bucket = mockBucketRepository.ById(bucketVersion.BucketId)
                                ?? throw new Exception("Bucket doesn't exist");

        return bucketVersion;
    }

    public int Create(BucketVersion entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.BucketVersions[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<BucketVersion> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketVersion entity)
    {
        try
        {
            _mockDatabase.BucketVersions[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<BucketVersion> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.BucketVersions.Remove(id) ? 1 : 0;
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