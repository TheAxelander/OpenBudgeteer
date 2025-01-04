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

public class BucketMovementDatabaseTest : BaseDatabaseTest<BucketMovement>
{
    private BucketGroup _testBucketGroup = new();
    private Bucket _testBucket1 = new();
    private Bucket _testBucket2 = new();
    
    protected override void CompareEntities(BucketMovement expected, BucketMovement actual)
    {
        Assert.Equal(expected.BucketId, actual.BucketId);
        Assert.Equal(expected.Amount, actual.Amount);
        Assert.Equal(expected.MovementDate, actual.MovementDate);
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
                    new MockBucketMovementRepository(mockDb),
                    new MockBucketRepository(mockDb),
                    new MockBucketGroupRepository(mockDb)
                },
                new object[]
                {
                    new BucketMovementRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketRepository(new DatabaseContext(MariaDbContextOptions)),
                    new BucketGroupRepository(new DatabaseContext(MariaDbContextOptions)),
                }
            };
        }
    }
    
    private List<BucketMovement> SetupTestData(
        IBucketMovementRepository bucketMovementRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        DeleteAllExtension<IBucketMovementRepository, BucketMovement>.DeleteAll(bucketMovementRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);

        _testBucketGroup = TestDataGenerator.Current.GenerateBucketGroup();
        bucketGroupRepository.Create(_testBucketGroup);
        
        _testBucket1 = TestDataGenerator.Current.GenerateBucket(_testBucketGroup);
        _testBucket2 = TestDataGenerator.Current.GenerateBucket(_testBucketGroup);
        bucketRepository.Create(_testBucket1);
        bucketRepository.Create(_testBucket2);
        
        var result = new List<BucketMovement>();
        for (var i = 1; i <= 4; i++)
        {
            var bucketMovement = i < 4
                ? TestDataGenerator.Current.GenerateBucketMovement(_testBucket1)
                : TestDataGenerator.Current.GenerateBucketMovement(_testBucket2);
            result.Add(bucketMovement);
            var repositoryResult = bucketMovementRepository.Create(bucketMovement);
            Assert.Equal(1, repositoryResult);
            Assert.NotEqual(Guid.Empty, bucketMovement.Id);
        }
        
        return result;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(
        IBucketMovementRepository bucketMovementRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var bucketMovements = SetupTestData(bucketMovementRepository, bucketRepository, bucketGroupRepository);
        RunChecks(bucketMovementRepository, bucketMovements);
    
        DeleteAllExtension<IBucketMovementRepository, BucketMovement>.DeleteAll(bucketMovementRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(
        IBucketMovementRepository bucketMovementRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var bucketMovements = SetupTestData(bucketMovementRepository, bucketRepository, bucketGroupRepository);

        foreach (var bucketMovement in bucketMovements)
        {
            var bucketMovementId = bucketMovement.Id;
            bucketMovement.BucketId = 
                bucketMovement.BucketId == _testBucket1.Id ? _testBucket2.Id : _testBucket1.Id;
            bucketMovement.Amount += 1;
            bucketMovement.MovementDate = bucketMovement.MovementDate.AddDays(1);
            var result = bucketMovementRepository.Update(bucketMovement);
            Assert.Equal(1, result);
            Assert.Equal(bucketMovementId, bucketMovement.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(bucketMovementRepository, bucketMovements);
        
        DeleteAllExtension<IBucketMovementRepository, BucketMovement>.DeleteAll(bucketMovementRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(
        IBucketMovementRepository bucketMovementRepository,
        IBucketRepository bucketRepository,
        IBucketGroupRepository bucketGroupRepository)
    {
        var bucketMovements = SetupTestData(bucketMovementRepository, bucketRepository, bucketGroupRepository);

        var deleteResult1 = bucketMovementRepository.Delete(bucketMovements.First().Id);
        var deleteResult2 = bucketMovementRepository.Delete(bucketMovements.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        bucketMovements.Remove(bucketMovements.First());
        bucketMovements.Remove(bucketMovements.Last());
        
        RunChecks(bucketMovementRepository, bucketMovements);
        
        DeleteAllExtension<IBucketMovementRepository, BucketMovement>.DeleteAll(bucketMovementRepository);
        DeleteAllExtension<IBucketRepository, Bucket>.DeleteAll(bucketRepository);
        DeleteAllExtension<IBucketGroupRepository, BucketGroup>.DeleteAll(bucketGroupRepository);
    }
}