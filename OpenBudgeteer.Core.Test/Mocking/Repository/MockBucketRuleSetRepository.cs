using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockBucketRuleSetRepository : IBucketRuleSetRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockBucketRuleSetRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<BucketRuleSet> All()
    {
        return _mockDatabase.BucketRuleSets.Values;
    }

    public IEnumerable<BucketRuleSet> AllWithIncludedEntities()
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockMappingRuleRepository = new MockMappingRuleRepository(_mockDatabase);
        var bucketRuleSets = All().ToList();
        foreach (var bucketRuleSet in bucketRuleSets)
        {
            bucketRuleSet.TargetBucket = mockBucketRepository.ById(bucketRuleSet.TargetBucketId) 
                                         ?? throw new Exception("Bucket doesn't exist");

            bucketRuleSet.MappingRules = new List<MappingRule>();
            foreach (var mappingRule in mockMappingRuleRepository
                         .All()
                         .Where(i => i.BucketRuleSetId == bucketRuleSet.Id))
            {
                bucketRuleSet.MappingRules.Add(mappingRule);    
            }
        }

        return bucketRuleSets;
    }

    public BucketRuleSet? ById(Guid id)
    {
        _mockDatabase.BucketRuleSets.TryGetValue(id, out var result);
        return result;
    }

    public BucketRuleSet? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRepository = new MockBucketRepository(_mockDatabase);
        var mockMappingRuleRepository = new MockMappingRuleRepository(_mockDatabase);
        var bucketRuleSet = ById(id);
        if (bucketRuleSet == null) return bucketRuleSet;
        bucketRuleSet.TargetBucket = mockBucketRepository.ById(bucketRuleSet.TargetBucketId) 
                                     ?? throw new Exception("Bucket doesn't exist");

        bucketRuleSet.MappingRules = new List<MappingRule>();
        foreach (var mappingRule in mockMappingRuleRepository
                     .All()
                     .Where(i => i.BucketRuleSetId == bucketRuleSet.Id))
        {
            bucketRuleSet.MappingRules.Add(mappingRule);    
        }

        return bucketRuleSet;
    }

    public int Create(BucketRuleSet entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.BucketRuleSets[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<BucketRuleSet> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketRuleSet entity)
    {
        try
        {
            _mockDatabase.BucketRuleSets[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<BucketRuleSet> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.BucketRuleSets.Remove(id) ? 1 : 0;
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