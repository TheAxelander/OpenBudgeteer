using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBucketService : GenericBucketService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketService> _logger;

    public DuckDbBucketService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbBucketService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketRepository(dbConnection);
    protected override IBucketVersionRepository CreateBucketVersionRepository(DbConnection dbConnection) => new DuckDbBucketVersionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(DbConnection dbConnection) => new DuckDbBudgetedTransactionRepository(dbConnection);
    protected override IBucketMovementRepository CreateBucketMovementRepository(DbConnection dbConnection) => new DuckDbBucketMovementRepository(dbConnection);
    protected override IBucketRuleSetRepository CreateBucketRuleSetRepository(DbConnection dbConnection) => new DuckDbBucketRuleSetRepository(dbConnection);
    
    public override Bucket Create(Bucket entity)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            if (entity.CurrentVersion is null) throw new EntityUpdateException("No Bucket Version defined.");

            entity.CurrentVersion.Version = 1;
            entity.BucketVersions = new List<BucketVersion>();
            entity.BucketVersions.Add(entity.CurrentVersion);
            
            baseRepository.Create(entity);
            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to create Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override Bucket Update(Bucket entity)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            if (entity.CurrentVersion is not null)
            {
                entity.BucketVersions = new List<BucketVersion>();
                if (entity.Id == Guid.Empty)
                {
                    // New Bucket - Create new Version
                    var newVersion = entity.CurrentVersion;
                    newVersion.Id = Guid.Empty;
                    newVersion.Version = 1;
                    entity.BucketVersions.Add(newVersion);
                }
                else
                {
                    var latestVersion = GetLatestVersion(entity.Id, DateOnly.FromDateTime(DateTime.Today));
                    if (entity.CurrentVersion.ValidFrom == latestVersion.ValidFrom)
                    {
                        // Change in same month, overwrite latest Version
                        latestVersion.BucketType = entity.CurrentVersion.BucketType;
                        latestVersion.BucketTypeXParam = entity.CurrentVersion.BucketTypeXParam;
                        latestVersion.BucketTypeYParam = entity.CurrentVersion.BucketTypeYParam;
                        latestVersion.BucketTypeZParam = entity.CurrentVersion.BucketTypeZParam;
                        latestVersion.Notes = entity.CurrentVersion.Notes;

                        entity.BucketVersions.Add(latestVersion);
                    }
                    else
                    {
                        // Create new Version
                        var newVersion = entity.CurrentVersion;
                        newVersion.Id = Guid.Empty;
                        newVersion.Version = latestVersion.Version + 1;
                        entity.BucketVersions.Add(newVersion);
                    }
                }
            }

            baseRepository.Update(entity);
            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to update Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            var budgetedTransactionRepository = CreateBudgetedTransactionRepository(dbContext);
            var bucketMovementRepository = CreateBucketMovementRepository(dbContext);
            var bucketRuleSetRepository = CreateBucketRuleSetRepository(dbContext);
            
            if (budgetedTransactionRepository.All().Any(i => i.BucketId == id) ||
                bucketMovementRepository.All().Any(i => i.BucketId == id))
            {
                throw new EntityUpdateException("Cannot delete a Bucket with assigned Transactions or Bucket Movements.");
            }
            
            // Delete Bucket
            baseRepository.Delete(id);
                            
            // Delete all BucketRuleSet which refer to this Bucket
            var bucketRuleSetIds = bucketRuleSetRepository
                .All()
                .Where(i => i.TargetBucketId == id)
                .Select(i => i.Id)
                .ToList();
            if (bucketRuleSetIds.Count != 0) bucketRuleSetRepository.DeleteRange(bucketRuleSetIds);
            
            transaction.Commit();
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to delete Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}