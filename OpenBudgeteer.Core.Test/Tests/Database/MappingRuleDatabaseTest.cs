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

public class MappingRuleDatabaseTest : BaseDatabaseTest<MappingRule>
{
    private BucketGroup _testBucketGroup = new();
    private Bucket _testBucket = new();
    private BucketRuleSet _testBucketRuleSet1 = new();
    private BucketRuleSet _testBucketRuleSet2 = new();
    
    protected override void CompareEntities(MappingRule expected, MappingRule actual)
    {
        Assert.Equal(expected.BucketRuleSetId, actual.BucketRuleSetId);
        Assert.Equal(expected.ComparisonField, actual.ComparisonField);
        Assert.Equal(expected.ComparisonType, actual.ComparisonType);
        Assert.Equal(expected.ComparisonValue, actual.ComparisonValue);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            var mockDb = new MockDatabase();
            return new[]
            {
                new object[]
                {
                    new MockMappingRuleRepository(mockDb),
                    new MockBucketRuleSetRepository(mockDb),
                    new MockBucketRepository(mockDb),
                    new MockBucketGroupRepository(mockDb)
                },
                new object[]
                {
                    new MappingRuleRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketRuleSetRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketGroupRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<MappingRule> SetupTestData(
        IMappingRuleRepository mappingRuleRepository,
        IBucketRuleSetRepository bucketRuleSetRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        DeleteAllExtension<IMappingRuleRepository, MappingRule>.DeleteAll(mappingRuleRepository);
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
        
        _testBucketGroup = TestDataGenerator.Current.GenerateBucketGroup();
        bucketGroupRepository.Create(_testBucketGroup);
        
        _testBucket = TestDataGenerator.Current.GenerateBucket(_testBucketGroup);
        bucketRepository.Create(_testBucket);

        _testBucketRuleSet1 = TestDataGenerator.Current.GenerateBucketRuleSet(_testBucket);
        _testBucketRuleSet2 = TestDataGenerator.Current.GenerateBucketRuleSet(_testBucket);
        bucketRuleSetRepository.Create(_testBucketRuleSet1);
        bucketRuleSetRepository.Create(_testBucketRuleSet2);

        var result = new List<MappingRule>();
        for (var i = 1; i <= 4; i++)
        {
            var mappingRule = i < 4
                ? TestDataGenerator.Current.GenerateMappingRule(_testBucketRuleSet1)
                : TestDataGenerator.Current.GenerateMappingRule(_testBucketRuleSet2);
            result.Add(mappingRule);
            var repositoryResult = mappingRuleRepository.Create(mappingRule);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, mappingRule.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IMappingRuleRepository mappingRuleRepository,
        IBucketRuleSetRepository bucketRuleSetRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var mappingRules = SetupTestData(
            mappingRuleRepository, bucketRuleSetRepository, bucketRepository, bucketGroupRepository);
        RunChecks(mappingRuleRepository, mappingRules);

        DeleteAllExtension<IMappingRuleRepository, MappingRule>.DeleteAll(mappingRuleRepository);
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IMappingRuleRepository mappingRuleRepository,
        IBucketRuleSetRepository bucketRuleSetRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var mappingRules = SetupTestData(
            mappingRuleRepository, bucketRuleSetRepository, bucketRepository, bucketGroupRepository);

        foreach (var mappingRule in mappingRules)
        {
            var mappingRuleId = mappingRule.Id;
            mappingRule.BucketRuleSetId =
                mappingRule.BucketRuleSetId == _testBucketRuleSet1.Id ? _testBucketRuleSet2.Id : _testBucketRuleSet1.Id;
            mappingRule.ComparisonField += 1;
            mappingRule.ComparisonType += 1;
            mappingRule.ComparisonValue += "Update";
            
            var result = mappingRuleRepository.Update(mappingRule);
            Assert.Equal(1, result);
            Assert.Equal(mappingRuleId, mappingRule.Id); // Check if no new Guid has been generated (no CREATE)
        }
        
        RunChecks(mappingRuleRepository, mappingRules);

        DeleteAllExtension<IMappingRuleRepository, MappingRule>.DeleteAll(mappingRuleRepository);
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IMappingRuleRepository mappingRuleRepository,
        IBucketRuleSetRepository bucketRuleSetRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var mappingRules = SetupTestData(
            mappingRuleRepository, bucketRuleSetRepository, bucketRepository, bucketGroupRepository);

        var deleteResult1 = mappingRuleRepository.Delete(mappingRules.First().Id);
        var deleteResult2 = mappingRuleRepository.Delete(mappingRules.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        mappingRules.Remove(mappingRules.First());
        mappingRules.Remove(mappingRules.Last());
        
        RunChecks(mappingRuleRepository, mappingRules);

        DeleteAllExtension<IMappingRuleRepository, MappingRule>.DeleteAll(mappingRuleRepository);
        DeleteAllExtension<IBucketRuleSetRepository, BucketRuleSet>.DeleteAll(bucketRuleSetRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
}