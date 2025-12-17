using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBucketMovementService<TDatabase> : GenericBaseService<BucketMovement, TDatabase>, IBucketMovementService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericBucketMovementService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IBucketMovementRepository CreateBaseRepository(TDatabase dbConnection);

    public IEnumerable<BucketMovement> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketMovementRepository = CreateBaseRepository(dbConnection);
            return bucketMovementRepository
                .All()
                .Where(i =>
                    i.MovementDate >= periodStart &&
                    i.MovementDate <= periodEnd)
                .ToList();
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
            using var dbConnection = CreateDbConnection();
            var bucketMovementRepository = CreateBaseRepository(dbConnection);
            return bucketMovementRepository
                .All()
                .Where(i =>
                    i.MovementDate >= periodStart &&
                    i.MovementDate <= periodEnd &&
                    i.BucketId == bucketId)
                .ToList();
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