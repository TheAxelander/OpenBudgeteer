using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBucketMovementRepository : IBucketMovementRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBucketMovementRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IQueryable<BucketMovement> All()
    {
        return _mockDatabase.BucketMovements.Values.AsQueryable();
    }

    public IQueryable<BucketMovement> AllWithIncludedEntities()
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketMovements = All().ToList();
        foreach (var bucketMovement in bucketMovements)
        {
            bucketMovement.Bucket = mockBucketRepository.ById(bucketMovement.BucketId) 
                                         ?? throw new Exception("Bucket doesn't exist");
        }

        return bucketMovements.AsQueryable();
    }

    public BucketMovement? ById(Guid id)
    {
        _mockDatabase.BucketMovements.TryGetValue(id, out var result);
        return result;
    }

    public BucketMovement? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var bucketMovement = ById(id);
        if (bucketMovement == null) return bucketMovement;
        bucketMovement.Bucket = mockBucketRepository.ById(bucketMovement.BucketId) 
                                ?? throw new Exception("Bucket doesn't exist");

        return bucketMovement;
    }

    public int Create(BucketMovement entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.BucketMovements[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<BucketMovement> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketMovement entity)
    {
        try
        {
            _mockDatabase.BucketMovements[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<BucketMovement> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.BucketMovements.Remove(id) ? 1 : 0;
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