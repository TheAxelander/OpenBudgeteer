using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBucketGroupService<TDatabase> : GenericBaseService<BucketGroup, TDatabase>, IBucketGroupService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericBucketGroupService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IBucketGroupRepository CreateBaseRepository(TDatabase dbConnection);

    public virtual BucketGroup GetWithBuckets(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            var result = bucketGroupRepository.ByIdWithIncludedEntities(id);
            if (result is null) throw new EntityNotFoundException($"Unable to find Bucket Group with the given id.");
            return result;
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

    public override IEnumerable<BucketGroup> GetAll()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            return bucketGroupRepository
                .AllWithIncludedEntities()
                .Where(i => i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001"))
                .OrderBy(i => i.Position)
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

    public virtual IEnumerable<BucketGroup> GetAllFull()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            return bucketGroupRepository
                .AllWithIncludedEntities()
                .OrderBy(i => i.Position)
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
    
    public virtual IEnumerable<BucketGroup> GetSystemBucketGroups()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            return bucketGroupRepository
                .AllWithIncludedEntities()
                .Where(i => i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001"))
                .OrderBy(i => i.Position) //In case in future there are multiple groups
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

    public override BucketGroup Create(BucketGroup entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            
            if (entity.Name == string.Empty) throw new EntityUpdateException("Bucket Group Name cannot be empty");
            var allGroups = GetAll().ToList();
            var lastNewPosition = allGroups.Count + 1;
            
            if (entity.Position > 0)
            {
                // Update positions of existing BucketGroups based on requested position
                // As GetAll excludes System Groups no check on 0 position required
                foreach (var bucketGroup in allGroups.Where(i => i.Position >= entity.Position)) 
                {
                    bucketGroup.Position++;
                    bucketGroupRepository.Update(bucketGroup);
                }
                
                // Fix a potential too large position number
                if (entity.Position > lastNewPosition) entity.Position = lastNewPosition;
            } 
            else
            {
                entity.Position = lastNewPosition;
            }
            bucketGroupRepository.Create(entity);

            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to create Bucket Group in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override BucketGroup Update(BucketGroup entity)
    {
        //TODO: Handle Position Update
        return base.Update(entity);
    }

    public override void Delete(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            var entity = bucketGroupRepository.ByIdWithIncludedEntities(id);
            if (entity is null) throw new EntityUpdateException("Bucket Group not found");
            if (entity.Buckets is not null && entity.Buckets.Any()) throw new EntityUpdateException("Bucket Group with Buckets cannot be deleted");

            var oldPosition = entity.Position;
            bucketGroupRepository.Delete(id);
            
            // Update Positions of other Bucket Groups
            foreach (var bucketGroup in GetAll().Where(i => i.Position > oldPosition))
            {
                bucketGroup.Position--;
                bucketGroupRepository.Update(bucketGroup);
            }
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to delete Bucket Group in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public virtual BucketGroup Move(Guid bucketGroupId, int positions)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            var (bucketGroup, updatedBucketGroups) = HandleMovement(bucketGroupId, positions);
            
            if (updatedBucketGroups.Any()) bucketGroupRepository.UpdateRange(updatedBucketGroups);
            return bucketGroup;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to move Bucket Group: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    /// <summary>
    /// Helper method so that it can be wrapped around a DB-transaction in inherited classes
    /// </summary>
    protected Tuple<BucketGroup, List<BucketGroup>> HandleMovement(Guid bucketGroupId, int positions)
    {
        // Create in an interim list to handle position updates
        var existingBucketGroups = new ObservableCollection<BucketGroup>();
        foreach (var group in GetAll().ToList())
        {
            existingBucketGroups.Add(group);
        }
        
        // Re-use existing reference in interim list of passed Bucket Group (see #282) 
        var bucketGroup = existingBucketGroups.First(i => i.Id == bucketGroupId);
        if (positions == 0) return new(bucketGroup, new());

        // Calculate new target position
        var bucketGroupCount = existingBucketGroups.Count();
        var targetPosition = bucketGroup.Position + positions;
        if (targetPosition < 1) targetPosition = 1;
        if (targetPosition > bucketGroupCount) targetPosition = bucketGroupCount;
        if (targetPosition == bucketGroup.Position) return new(bucketGroup, new()); // Group is already at the end or top. No further action

        // Move Group in interim list
        existingBucketGroups.Move(bucketGroup.Position - 1, targetPosition - 1);
                    
        // Update Position number for each group
        var newPosition = 1;
        foreach (var group in existingBucketGroups)
        {
            group.Position = newPosition;
            newPosition++;
        }
        
        return new(bucketGroup, existingBucketGroups.ToList());
    }
}