using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.EFCore;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketGroupService : GenericBucketGroupService<DatabaseContext>
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly ILogger<EFCoreBucketGroupService> _logger;
    
    public EFCoreBucketGroupService(
        IDbContextFactory<DatabaseContext> dbContextFactory, 
        ILogger<EFCoreBucketGroupService> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    protected override DatabaseContext CreateDbConnection() => _dbContextFactory.CreateDbContext();
    protected override EFCoreBucketGroupRepository CreateBaseRepository(DatabaseContext dbConnection) => new (dbConnection);
    
    public override BucketGroup Move(Guid bucketGroupId, int positions)
    {
        using var dbConnection = _dbContextFactory.CreateDbContext();
        using var transaction = dbConnection.Database.BeginTransaction();
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