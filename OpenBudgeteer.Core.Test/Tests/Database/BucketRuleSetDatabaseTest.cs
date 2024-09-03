using System;
using System.Collections.Generic;
using System.Linq;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Test.Common;
using OpenBudgeteer.Core.Test.Mocking;
using OpenBudgeteer.Core.Test.Mocking.Repository;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.Database;

public class BucketRuleSetDatabaseTest : BaseDatabaseTest<BucketRuleSet>
{
    private readonly Bucket _incomeBucket = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
        Name = "Income"
    };
    private readonly Bucket _transferBucket = new()
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), 
        Name = "Transfer"
    };
    
    protected override void CompareEntities(BucketRuleSet expected, BucketRuleSet actual)
    {
        Assert.Equal(expected.Priority, actual.Priority);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.TargetBucketId, actual.TargetBucketId);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new MockBucketRuleSetRepository(new MockDatabase())
                },
                new object[]
                {
                    new BucketRuleSetRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<BucketRuleSet> SetupTestData(IBucketRuleSetRepository bucketRuleSetRepository)
    {
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
        
        var bucketRuleSets = new List<BucketRuleSet>();
        for (var i = 1; i <= 4; i++)
        {
            var date = DateTime.Now.AddDays(i);
            bucketRuleSets.Add(new()
            {
                Priority = i,
                Name = $"RuleSet {i}", 
                TargetBucketId = i < 4 ? _incomeBucket.Id : _transferBucket.Id
            });
        }
        
        foreach (var bucketRuleSet in bucketRuleSets)
        {
            var result = bucketRuleSetRepository.Create(bucketRuleSet);
            Assert.Equal(1, result);
            Assert.NotEqual(Guid.Empty, bucketRuleSet.Id);
        }

        return bucketRuleSets;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(IBucketRuleSetRepository bucketRuleSetRepository)
    {
        var bucketRuleSets = SetupTestData(bucketRuleSetRepository);
        RunChecks(bucketRuleSetRepository, bucketRuleSets);
    
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(IBucketRuleSetRepository bucketRuleSetRepository)
    {
        var bucketRuleSets = SetupTestData(bucketRuleSetRepository);

        foreach (var bucketRuleSet in bucketRuleSets)
        {
            var bucketRuleSetId = bucketRuleSet.Id;
            bucketRuleSet.Priority += 1;
            bucketRuleSet.Name += "Update";
            bucketRuleSet.TargetBucketId = 
                bucketRuleSet.TargetBucketId == _incomeBucket.Id ? _transferBucket.Id : _incomeBucket.Id;
            var result = bucketRuleSetRepository.Update(bucketRuleSet);
            Assert.Equal(1, result);
            Assert.Equal(bucketRuleSetId, bucketRuleSet.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(bucketRuleSetRepository, bucketRuleSets);
        
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(IBucketRuleSetRepository bucketRuleSetRepository)
    {
        var bucketRuleSets = SetupTestData(bucketRuleSetRepository);

        var deleteResult1 = bucketRuleSetRepository.Delete(bucketRuleSets.First().Id);
        var deleteResult2 = bucketRuleSetRepository.Delete(bucketRuleSets.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        bucketRuleSets.Remove(bucketRuleSets.First());
        bucketRuleSets.Remove(bucketRuleSets.Last());
        
        RunChecks(bucketRuleSetRepository, bucketRuleSets);
        
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
    }
}