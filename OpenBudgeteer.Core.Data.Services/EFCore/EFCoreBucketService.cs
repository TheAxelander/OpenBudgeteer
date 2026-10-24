using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketService : GenericBucketService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketService> _logger;

    public EFCoreBucketService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreBucketRepository CreateBaseRepository(DatabaseContext dbConnection) => new (dbConnection);
    protected override EFCoreBucketVersionRepository CreateBucketVersionRepository(DatabaseContext dbConnection) => new (dbConnection);
    protected override EFCoreBudgetedTransactionRepository CreateBudgetedTransactionRepository(DatabaseContext dbConnection) => new (dbConnection);
    protected override EFCoreBucketMovementRepository CreateBucketMovementRepository(DatabaseContext dbConnection) => new (dbConnection);
    protected override EFCoreBucketRuleSetRepository CreateBucketRuleSetRepository(DatabaseContext dbConnection) => new (dbConnection);

    public override Bucket Create(Bucket entity)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
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
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
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
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
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