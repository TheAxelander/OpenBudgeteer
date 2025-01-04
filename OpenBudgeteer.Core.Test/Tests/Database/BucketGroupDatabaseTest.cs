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

public class BucketGroupDatabaseTest : BaseDatabaseTest<BucketGroup>
{
    protected override void CompareEntities(BucketGroup expected, BucketGroup actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Position, actual.Position);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            return new[]
            {
                new object[] { new MockBucketGroupRepository(new MockDatabase()) },
                new object[] { new BucketGroupRepository(new DatabaseContext(MariaDbContextOptions)) }
            };
        }
    }
    
    private List<BucketGroup> SetupTestData(IBucketGroupRepository bucketGroupRepository)
    {
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
        
        var result = new List<BucketGroup>();
        for (var i = 1; i <= 4; i++)
        {
            var bucketGroup = TestDataGenerator.Current.GenerateBucketGroup();
            result.Add(bucketGroup);
            var repositoryResult = bucketGroupRepository.Create(bucketGroup);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, bucketGroup.Id);
        }

        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(IBucketGroupRepository baseRepository)
    {
        var bucketGroups = SetupTestData(baseRepository);
        RunChecks(baseRepository, bucketGroups);
    
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(baseRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(IBucketGroupRepository baseRepository)
    {
        var bucketGroups = SetupTestData(baseRepository);

        foreach (var bucketGroup in bucketGroups)
        {
            var bucketGroupId = bucketGroup.Id;
            bucketGroup.Name += "Update";
            bucketGroup.Position += 1;
            var result = baseRepository.Update(bucketGroup);
            Assert.Equal(1, result);
            Assert.Equal(bucketGroupId, bucketGroup.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(baseRepository, bucketGroups);
        
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(baseRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(IBucketGroupRepository baseRepository)
    {
        var bucketGroups = SetupTestData(baseRepository);

        var deleteResult1 = baseRepository.Delete(bucketGroups.First().Id);
        var deleteResult2 = baseRepository.Delete(bucketGroups.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        bucketGroups.Remove(bucketGroups.First());
        bucketGroups.Remove(bucketGroups.Last());
        
        RunChecks(baseRepository, bucketGroups);
        
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(baseRepository);
    }
}