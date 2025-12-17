using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketRuleSetService : EFCoreBaseService<BucketRuleSet>, IBucketRuleSetService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketRuleSetService> _logger;

    public EFCoreBucketRuleSetService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketRuleSetService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericBucketRuleSetService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketRuleSetService(
            new BucketRuleSetRepository(dbContext),
            new MappingRuleRepository(dbContext));
    }

    public override BucketRuleSet Get(Guid id)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.Get(id);
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public override IEnumerable<BucketRuleSet> GetAll()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetMappingRules(bucketRuleSetId);
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var results = baseService.Update(entity);
            transaction.Commit();
            return results;
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
        var baseService = CreateBaseService(dbContext);
        try
        {
            baseService.Delete(id);
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