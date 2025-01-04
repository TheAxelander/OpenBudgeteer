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

public class BucketDatabaseTest : BaseDatabaseTest<Bucket>
{
    private BucketGroup _testBucketGroup1 = new();
    private BucketGroup _testBucketGroup2 = new();
    
    protected override void CompareEntities(Bucket expected, Bucket actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.BucketGroupId, actual.BucketGroupId);
        Assert.Equal(expected.ColorCode, actual.ColorCode);
        Assert.Equal(expected.TextColorCode, actual.TextColorCode);
        Assert.Equal(expected.ValidFrom, actual.ValidFrom);
        Assert.Equal(expected.IsInactive, actual.IsInactive);
        Assert.Equal(expected.IsInactiveFrom, actual.IsInactiveFrom);

        Assert.NotNull(expected.BucketVersions);
        Assert.NotNull(actual.BucketVersions);
        Assert.Equal(expected.BucketVersions.Count, actual.BucketVersions.Count);
        var expectedBucketVersion = expected.BucketVersions.First();
        var actualBucketVersion = actual.BucketVersions.First();
        Assert.Equal(expectedBucketVersion.BucketId, actualBucketVersion.BucketId);
        Assert.Equal(expectedBucketVersion.Version, actualBucketVersion.Version);
        Assert.Equal(expectedBucketVersion.BucketType, actualBucketVersion.BucketType);
        Assert.Equal(expectedBucketVersion.BucketTypeXParam, actualBucketVersion.BucketTypeXParam);
        Assert.Equal(expectedBucketVersion.BucketTypeYParam, actualBucketVersion.BucketTypeYParam);
        Assert.Equal(expectedBucketVersion.BucketTypeZParam, actualBucketVersion.BucketTypeZParam);
        Assert.Equal(expectedBucketVersion.Notes, actualBucketVersion.Notes);
        Assert.Equal(expectedBucketVersion.ValidFrom, actualBucketVersion.ValidFrom);
    }

    protected override void RunChecks(IBaseRepository<Bucket> baseRepository, List<Bucket> testEntities)
    {
        var dbEntities = baseRepository.AllWithIncludedEntities().ToList();
        foreach (var testEntity in testEntities)
        {
            var dbEntity = dbEntities.First(i => i.Id == testEntity.Id);
            CompareEntities(testEntity, dbEntity);
        }
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
                    new MockBucketRepository(mockDb),
                    new MockBucketGroupRepository(mockDb)
                },
                new object[]
                {
                    new BucketRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketGroupRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<Bucket> SetupTestData(
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);

        _testBucketGroup1 = TestDataGenerator.Current.GenerateBucketGroup();
        _testBucketGroup2 = TestDataGenerator.Current.GenerateBucketGroup();

        bucketGroupRepository.Create(_testBucketGroup1);
        bucketGroupRepository.Create(_testBucketGroup2);

        var result = new List<Bucket>();
        for (var i = 1; i <= 4; i++)
        {
            var bucket = i < 4 
                ? TestDataGenerator.Current.GenerateBucket(_testBucketGroup1)
                : TestDataGenerator.Current.GenerateBucket(_testBucketGroup2); 
            result.Add(bucket);
            var repositoryResult = bucketRepository.Create(bucket);
            Assert.Equal(2, repositoryResult); // Bucket + BucketVersion
            Assert.NotEqual(Guid.Empty, bucket.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var buckets = SetupTestData(bucketRepository, bucketGroupRepository);
        RunChecks(bucketRepository, buckets);

        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }

    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var buckets = SetupTestData(bucketRepository, bucketGroupRepository);

        foreach (var bucket in buckets)
        {
            var bucketId = bucket.Id;
            bucket.Name += "Update";
            bucket.BucketGroupId = _testBucketGroup2.Id;
            bucket.ColorCode += "Update";
            bucket.TextColorCode += "Update";
            bucket.ValidFrom = bucket.ValidFrom.AddDays(1);
            bucket.IsInactive = !bucket.IsInactive;
            bucket.IsInactiveFrom = bucket.IsInactive 
                ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) 
                : DateTime.MinValue;

            var result = bucketRepository.Update(bucket);
            Assert.Equal(1, result);
            Assert.Equal(bucketId, bucket.Id); // Check if no new Guid has been generated (no CREATE)
        }
        
        RunChecks(bucketRepository, buckets);
        
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }

    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var buckets = SetupTestData(bucketRepository, bucketGroupRepository);
        
        var deleteResult1 = bucketRepository.Delete(buckets.First().Id);
        var deleteResult2 = bucketRepository.Delete(buckets.Last().Id);
        Assert.Equal(2, deleteResult1); // Bucket + BucketVersion
        Assert.Equal(2, deleteResult2); // Bucket + BucketVersion
        buckets.Remove(buckets.First());
        buckets.Remove(buckets.Last());
        
        RunChecks(bucketRepository, buckets);
        
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
}