using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketMovementService : EFCoreBaseService<BucketMovement>, IBucketMovementService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketMovementService> _logger;
    
    public EFCoreBucketMovementService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketMovementService> logger) : base(dbContextFactory, logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override GenericBucketMovementService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketMovementService(new BucketMovementRepository(dbContext));
    }

    public IEnumerable<BucketMovement> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll(periodStart, periodEnd);
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

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateOnly.MinValue, DateOnly.MaxValue);
    }

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromBucket(bucketId, periodStart, periodEnd);
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
}