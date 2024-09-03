using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Test.Mocking.Repository;

public class MockMappingRuleRepository : IMappingRuleRepository
{
    private readonly MockDatabase _mockDatabase;

    public MockMappingRuleRepository(MockDatabase mockDatabase)
    {
        _mockDatabase = mockDatabase;
    }

    public IEnumerable<MappingRule> All()
    {
        return _mockDatabase.MappingRules.Values;
    }

    public IEnumerable<MappingRule> AllWithIncludedEntities()
    {
        var mockBucketRuleSetRepository = new MockBucketRuleSetRepository(_mockDatabase);
        var mappingRules = All().ToList();
        foreach (var mappingRule in mappingRules)
        {
            mappingRule.BucketRuleSet = mockBucketRuleSetRepository.ById(mappingRule.BucketRuleSetId) 
                                         ?? throw new Exception("BucketRuleSet doesn't exist");
        }

        return mappingRules;
    }

    public MappingRule? ById(Guid id)
    {
        _mockDatabase.MappingRules.TryGetValue(id, out var result);
        return result;
    }

    public MappingRule? ByIdWithIncludedEntities(Guid id)
    {
        var mockBucketRuleSetRepository = new MockBucketRuleSetRepository(_mockDatabase);
        var mappingRule = ById(id);
        if (mappingRule == null) return mappingRule;
        mappingRule.BucketRuleSet = mockBucketRuleSetRepository.ById(mappingRule.BucketRuleSetId) 
                                    ?? throw new Exception("BucketRuleSet doesn't exist");

        return mappingRule;
    }

    public int Create(MappingRule entity)
    {
        entity.Id = Guid.NewGuid();
        _mockDatabase.MappingRules[entity.Id] = entity;
        return 1;
    }

    public int CreateRange(IEnumerable<MappingRule> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(MappingRule entity)
    {
        try
        {
            _mockDatabase.MappingRules[entity.Id] = entity;
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 0;
        }
    }

    public int UpdateRange(IEnumerable<MappingRule> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        try
        {
            return _mockDatabase.MappingRules.Remove(id) ? 1 : 0;
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