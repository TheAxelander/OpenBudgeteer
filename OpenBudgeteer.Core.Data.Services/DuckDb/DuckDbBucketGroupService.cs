using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBucketGroupService : GenericBucketGroupService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketGroupService> _logger;

    public DuckDbBucketGroupService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbBucketGroupService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketGroupRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketGroupRepository(dbConnection);
    
    public override BucketGroup Move(Guid bucketGroupId, int positions)
    {
        using var dbConnection = CreateDbConnection();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            var (bucketGroup, updatedBucketGroups) = HandleMovement(bucketGroupId, positions);
            
            if (updatedBucketGroups.Any()) bucketGroupRepository.UpdateRange(updatedBucketGroups);
            transaction.Commit();
            return bucketGroup;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to move Bucket Group: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}