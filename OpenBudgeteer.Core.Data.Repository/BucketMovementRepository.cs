using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketMovementRepository : IBucketMovementRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BucketMovementRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<BucketMovement> All() => DatabaseContext.BucketMovement
        .AsNoTracking();
    
    public IQueryable<BucketMovement> AllWithIncludedEntities() => DatabaseContext.BucketMovement
        .Include(i => i.Bucket)
        .AsNoTracking();

    public BucketMovement? ById(Guid id) => DatabaseContext.BucketMovement
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());
    
    public BucketMovement? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BucketMovement
        .Include(i => i.Bucket)
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());

    public int Create(BucketMovement entity)
    {
        DatabaseContext.BucketMovement.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BucketMovement> entities)
    {
        DatabaseContext.BucketMovement.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BucketMovement entity)
    {
        DatabaseContext.BucketMovement.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<BucketMovement> entities)
    {
        DatabaseContext.BucketMovement.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.BucketMovement.FirstOrDefault(i => i.Id.ToString() == id.ToString());
        if (entity == null) throw new Exception($"BucketMovement with id {id} not found.");

        DatabaseContext.BucketMovement.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.BucketMovement.Where(i => ids.Select(j => j.ToString()).Contains(i.Id.ToString()));
        if (!entities.Any()) throw new Exception($"No BucketMovement found with passed IDs.");

        DatabaseContext.BucketMovement.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}