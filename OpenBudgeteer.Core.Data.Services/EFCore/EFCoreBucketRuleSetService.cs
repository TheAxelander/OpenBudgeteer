using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketRuleSetService : GenericBucketRuleSetService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketRuleSetService> _logger;

    public EFCoreBucketRuleSetService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketRuleSetService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    
    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreBucketRuleSetRepository CreateBaseRepository(DatabaseContext dbConnection) => new(dbConnection);
    protected override EFCoreMappingRuleRepository CreateMappingRuleRepository(DatabaseContext dbConnection) => new(dbConnection);

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = CreateBaseRepository(dbContext);
            var mappingRuleRepository = CreateMappingRuleRepository(dbContext);
            
            // Check if Mapping Rules need to be deleted
            var deletedIds = 
                // Collect database entities
                mappingRuleRepository.All()
                    .Where(i => i.BucketRuleSetId == entity.Id)
                    .ToList()
                    // Select which of the database IDs are no longer available in entity
                    .Where(i => entity.MappingRules is not null && entity.MappingRules
                        .All(j => j.Id != i.Id))
                    .Select(i => i.Id)
                    .ToList();
            if (deletedIds.Count != 0)
            {
                var result = mappingRuleRepository.DeleteRange(deletedIds);
                if (result != deletedIds.Count) 
                    throw new EntityUpdateException("Unable to delete old Mapping Rules of that Rule Set");
            }
            
            // Update BucketRuleSet including MappingRules
            bucketRuleSetRepository.Update(entity);
            
            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to update Rule Set: {e.Message}", _logger);
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
            var bucketRuleSetRepository = CreateBaseRepository(dbContext);
            var mappingRuleRepository = CreateMappingRuleRepository(dbContext);
            
            // Delete all existing Mapping Rules
            mappingRuleRepository.DeleteRange(mappingRuleRepository
                .All()
                .Where(i => i.BucketRuleSetId == id)
                .Select(i => i.Id)
                .ToList());
            
            // Delete BucketRuleSet
            bucketRuleSetRepository.Delete(id);
            transaction.Commit();
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to delete Rule Set: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}