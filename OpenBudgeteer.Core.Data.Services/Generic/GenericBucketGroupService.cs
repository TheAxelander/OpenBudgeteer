using System.Collections.ObjectModel;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBucketGroupService : GenericBaseService<BucketGroup>, IBucketGroupService
{
    private readonly IBucketGroupRepository _bucketGroupRepository;
    
    public GenericBucketGroupService(
        IBucketGroupRepository bucketGroupRepository) : base(bucketGroupRepository)
    {
        _bucketGroupRepository = bucketGroupRepository;
    }

    public BucketGroup GetWithBuckets(Guid id)
    {
        var result = _bucketGroupRepository.ByIdWithIncludedEntities(id);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }

    public override IEnumerable<BucketGroup> GetAll()
    {
        return _bucketGroupRepository
            .AllWithIncludedEntities()
            .Where(i => i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .OrderBy(i => i.Position)
            .ToList();
    }

    public IEnumerable<BucketGroup> GetAllFull()
    {
        return _bucketGroupRepository
            .AllWithIncludedEntities()
            .OrderBy(i => i.Position)
            .ToList();
    }
    
    public IEnumerable<BucketGroup> GetSystemBucketGroups()
    {
        return _bucketGroupRepository
            .AllWithIncludedEntities()
            .Where(i => i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .OrderBy(i => i.Position) //In case in future there are multiple groups
            .ToList();
    }

    public override BucketGroup Create(BucketGroup entity)
    {
        var allGroups = GetAll().ToList();
        var lastNewPosition = allGroups.Count + 1;
            
        if (entity.Position > 0)
        {
            // Update positions of existing BucketGroups based on requested position
            // As GetAll excludes System Groups no check on 0 position required
            foreach (var bucketGroup in allGroups.Where(i => i.Position >= entity.Position)) 
            {
                bucketGroup.Position++;
                _bucketGroupRepository.Update(bucketGroup);
            }
                
            // Fix a potential too large position number
            if (entity.Position > lastNewPosition) entity.Position = lastNewPosition;
                
            _bucketGroupRepository.Create(entity);
        } 
        else
        {
            entity.Position = lastNewPosition;
            _bucketGroupRepository.Create(entity);
        }
            
        return entity;
    }

    public override BucketGroup Update(BucketGroup entity)
    {
        //TODO: Handle Position Update
        return base.Update(entity);
    }

    public override void Delete(Guid id)
    {
        var entity = _bucketGroupRepository.ByIdWithIncludedEntities(id);
        if (entity == null) throw new Exception("BucketGroup not found");
        if (entity.Buckets != null && entity.Buckets.Any()) throw new Exception("BucketGroup with Buckets cannot be deleted");

        var oldPosition = entity.Position;
        _bucketGroupRepository.Delete(id);
            
        // Update Positions of other Bucket Groups
        foreach (var bucketGroup in GetAll().Where(i => i.Position > oldPosition))
        {
            bucketGroup.Position--;
            _bucketGroupRepository.Update(bucketGroup);
        }
    }

    public BucketGroup Move(Guid bucketGroupId, int positions)
    {
        var bucketGroup = Get(bucketGroupId);
        if (positions == 0) return bucketGroup;

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
            _bucketGroupRepository.Update(group);
            if (group.Id == bucketGroupId) bucketGroup = group; // Use correct object reference for final return
            newPosition++;
        }

        return bucketGroup;
    }
}