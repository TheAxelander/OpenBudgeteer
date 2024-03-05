using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketGroupService : BaseService<BucketGroup>, IBucketGroupService
{
    internal BucketGroupService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new BucketGroupRepository(new DatabaseContext(dbContextOptions)))
    {
    }

    public BucketGroup GetWithBuckets(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketGroupRepository(dbContext);
        
            var result = repository.ByIdWithIncludedEntities(id);
            if (result == null) throw new Exception($"{typeof(BucketGroup)} not found in database");
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override IEnumerable<BucketGroup> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketGroupRepository(dbContext);
            return repository
                .AllWithIncludedEntities()
                .Where(i => i.Id.ToString() != "00000000-0000-0000-0000-000000000001")
                .OrderBy(i => i.Position)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BucketGroup> GetAllFull()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketGroupRepository(dbContext);
            return repository
                .AllWithIncludedEntities()
                .OrderBy(i => i.Position)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    public IEnumerable<BucketGroup> GetSystemBucketGroups()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketGroupRepository(dbContext);
            return repository.AllWithIncludedEntities()
                .Where(i => i.Id.ToString() == "00000000-0000-0000-0000-000000000001")
                .OrderBy(i => i.Position) //In case in future there are multiple groups
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override BucketGroup Create(BucketGroup entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var repository = new BucketGroupRepository(dbContext);
            
            var allGroups = GetAll().ToList();
            var lastNewPosition = allGroups.Count + 1;
            
            if (entity.Position > 0)
            {
                // Update positions of existing BucketGroups based on requested position
                // As GetAll excludes System Groups no check on 0 position required
                foreach (var bucketGroup in allGroups.Where(i => i.Position >= entity.Position)) 
                {
                    bucketGroup.Position++;
                    repository.Update(bucketGroup);
                }
                
                // Fix a potential too large position number
                if (entity.Position > lastNewPosition) entity.Position = lastNewPosition;
                
                repository.Create(entity);
            } 
            else
            {
                entity.Position = lastNewPosition;
                repository.Create(entity);
            }
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override BucketGroup Update(BucketGroup entity)
    {
        //TODO Handle Position Update
        return base.Update(entity);
    }

    public override void Delete(Guid id)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketGroupRepository = new BucketGroupRepository(dbContext);

            var entity = bucketGroupRepository.ByIdWithIncludedEntities(id);
            if (entity == null) throw new Exception("BucketGroup not found");
            if (entity.Buckets.Any()) throw new Exception("BucketGroup with Buckets cannot be deleted");

            var oldPosition = entity.Position;
            bucketGroupRepository.Delete(id);
            
            // Update Positions of other Bucket Groups
            foreach (var bucketGroup in GetAll().Where(i => i.Position > oldPosition))
            {
                bucketGroup.Position--;
                bucketGroupRepository.Update(bucketGroup);
            }
            
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public BucketGroup Move(Guid bucketGroupId, int positions)
    {
        var bucketGroup = Get(bucketGroupId);
        if (positions == 0) return bucketGroup;

        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var repository = new BucketGroupRepository(dbContext);
            
            // Create in an interim List for later use
            var existingBucketGroups = new ObservableCollection<BucketGroup>();
            foreach (var group in GetAll().ToList())
            {
                existingBucketGroups.Add(group);
            }
            var bucketGroupCount = existingBucketGroups.Count();
            var targetPosition = bucketGroup.Position + positions;
            if (targetPosition < 1) targetPosition = 1;
            if (targetPosition > bucketGroupCount) targetPosition = bucketGroupCount;
            if (targetPosition == bucketGroup.Position) return bucketGroup; // Group is already at the end or top. No further action

            // Move Group in interim List
            existingBucketGroups.Move(bucketGroup.Position - 1, targetPosition - 1);
                    
            // Update Position number
            var newPosition = 1;
            foreach (var group in existingBucketGroups)
            {
                group.Position = newPosition;
                repository.Update(group);
                if (group.Id == bucketGroupId) bucketGroup = group; // Use correct object reference for final return
                newPosition++;
            }

            transaction.Commit();
            return bucketGroup;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
}