using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketService : EFCoreBaseService<Bucket>, IBucketService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketService> _logger;

    public EFCoreBucketService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericBucketService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketService(
            new BucketRepository(dbContext),
            new BucketVersionRepository(dbContext),
            new BudgetedTransactionRepository(dbContext),
            new BucketMovementRepository(dbContext),
            new BucketRuleSetRepository(dbContext));
    }

    public Bucket GetWithLatestVersion(Guid id)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetWithLatestVersion(id);
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

    public IEnumerable<Bucket> GetAllWithoutSystemBuckets()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllWithoutSystemBuckets();
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
    
    public override IEnumerable<Bucket> GetAll()
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

    public IEnumerable<Bucket> GetSystemBuckets()
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetSystemBuckets();
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

    public IEnumerable<Bucket> GetActiveBuckets(DateOnly validFrom)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetActiveBuckets(validFrom);
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
    
    public BucketVersion GetLatestVersion(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetLatestVersion(bucketId, yearMonth);
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

    public BucketFigures GetFigures(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetFigures(bucketId, yearMonth);
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

    public decimal GetBalance(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetBalance(bucketId, yearMonth);
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

    public BucketFigures GetInAndOut(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetInAndOut(bucketId, yearMonth);
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

    public override Bucket Create(Bucket entity)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var result = baseService.Create(entity);
            transaction.Commit();
            return result;
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
        var baseService = CreateBaseService(dbContext);
        try
        {
            var result = baseService.Update(entity);
            transaction.Commit();
            return result;
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

    public void Close(Guid id, DateOnly yearMonth)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            baseService.Close(id, yearMonth);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to close Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
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
            throw new ServiceException($"Unable to delete Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateOnly movementDate)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.CreateMovement(bucketId, amount, movementDate);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create Movement: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}