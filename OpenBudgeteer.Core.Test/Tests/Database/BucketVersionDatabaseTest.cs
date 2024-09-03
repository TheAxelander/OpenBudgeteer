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

public class BucketVersionDatabaseTest : BaseDatabaseTest<BucketVersion>
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
    
    protected override void CompareEntities(BucketVersion expected, BucketVersion actual)
    {
        Assert.Equal(expected.BucketId, actual.BucketId);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.BucketType, actual.BucketType);
        Assert.Equal(expected.BucketTypeXParam, actual.BucketTypeXParam);
        Assert.Equal(expected.BucketTypeYParam, actual.BucketTypeYParam);
        Assert.Equal(expected.BucketTypeZParam, actual.BucketTypeZParam);
        Assert.Equal(expected.Notes, actual.Notes);
        Assert.Equal(expected.ValidFrom, actual.ValidFrom);
    }
    
    public static IEnumerable<object[]> TestData_Repository
    {
        get
        {
            return new[]
            {
                new object[]
                {
                    new MockBucketVersionRepository(new MockDatabase())
                },
                new object[]
                {
                    new BucketVersionRepository(new DatabaseContext(MariaDbContextOptions))
                }
            };
        }
    }
    
    private List<BucketVersion> SetupTestData(IBucketVersionRepository bucketVersionRepository)
    {
        DeleteAllExtension<IBucketVersionRepository, BucketVersion>.DeleteAll(bucketVersionRepository);
        
        var bucketVersions = new List<BucketVersion>();
        for (var i = 1; i <= 4; i++)
        {
            var date = DateTime.Now.AddDays(i);
            bucketVersions.Add(new()
            {
                BucketId = i < 4 ? _incomeBucket.Id : _transferBucket.Id,
                Version = 1,
                BucketType = i,
                BucketTypeXParam = i,
                BucketTypeYParam = i,
                BucketTypeZParam = new DateTime(date.Year, date.Month, date.Day),
                Notes = $"Notes {i}",
                ValidFrom = new DateTime(date.Year, date.Month, date.Day)
            });
        }
        
        foreach (var bucketVersion in bucketVersions)
        {
            var result = bucketVersionRepository.Create(bucketVersion);
            Assert.Equal(1, result);
            Assert.NotEqual(Guid.Empty, bucketVersion.Id);
        }

        return bucketVersions;
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Create(IBucketVersionRepository bucketVersionRepository)
    {
        var bucketVersions = SetupTestData(bucketVersionRepository);
        RunChecks(bucketVersionRepository, bucketVersions);
    
        DeleteAllExtension<IBucketVersionRepository, BucketVersion>.DeleteAll(bucketVersionRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Update(IBucketVersionRepository bucketVersionRepository)
    {
        var bucketVersions = SetupTestData(bucketVersionRepository);

        foreach (var bucketVersion in bucketVersions)
        {
            var bucketVersionId = bucketVersion.Id;
            bucketVersion.BucketId = 
                bucketVersion.BucketId == _incomeBucket.Id ? _transferBucket.Id : _incomeBucket.Id;
            bucketVersion.Version += 1;
            bucketVersion.BucketType += 1;
            bucketVersion.BucketTypeXParam += 1;
            bucketVersion.BucketTypeYParam += 1;
            bucketVersion.BucketTypeZParam = bucketVersion.BucketTypeZParam.AddDays(1);
            bucketVersion.Notes += "Update";
            bucketVersion.ValidFrom = bucketVersion.ValidFrom.AddDays(1);
            var result = bucketVersionRepository.Update(bucketVersion);
            Assert.Equal(1, result);
            Assert.Equal(bucketVersionId, bucketVersion.Id); // Check if no new Guid has been generated (no CREATE)
        }

        RunChecks(bucketVersionRepository, bucketVersions);
        
        DeleteAllExtension<IBucketVersionRepository, BucketVersion>.DeleteAll(bucketVersionRepository);
    }
    
    [Theory]
    [MemberData(nameof(TestData_Repository))]
    public void Delete(IBucketVersionRepository bucketVersionRepository)
    {
        var bucketVersions = SetupTestData(bucketVersionRepository);

        var deleteResult1 = bucketVersionRepository.Delete(bucketVersions.First().Id);
        var deleteResult2 = bucketVersionRepository.Delete(bucketVersions.Last().Id);
        Assert.Equal(1, deleteResult1);
        Assert.Equal(1, deleteResult2);
        bucketVersions.Remove(bucketVersions.First());
        bucketVersions.Remove(bucketVersions.Last());
        
        RunChecks(bucketVersionRepository, bucketVersions);
        
        DeleteAllExtension<IBucketVersionRepository, BucketVersion>.DeleteAll(bucketVersionRepository);
    }
}