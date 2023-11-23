using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketGroupRepository : IBucketGroupRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BucketGroupRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<BucketGroup> All() => DatabaseContext.BucketGroup
        .AsNoTracking();
    
    public IQueryable<BucketGroup> AllWithIncludedEntities() => DatabaseContext.BucketGroup
        .Include(i => i.Buckets)
        .AsNoTracking();

    public BucketGroup? ById(Guid id) => DatabaseContext.BucketGroup
        .FirstOrDefault(i => i.Id == id);
    
    public BucketGroup? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BucketGroup
        .Include(i => i.Buckets)
        .FirstOrDefault(i => i.Id == id);

    public int Create(BucketGroup entity)
    {
        DatabaseContext.BucketGroup.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BucketGroup> entities)
    {
        DatabaseContext.BucketGroup.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BucketGroup entity)
    {
        DatabaseContext.BucketGroup.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<BucketGroup> entities)
    {
        DatabaseContext.BucketGroup.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        if (id == Guid.Parse("00000000-0000-0000-0000-000000000001")) return 0;
        
        var entity = DatabaseContext.BucketGroup
            .Include(i => i.Buckets)
            .FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"BucketGroup with id {id} not found.");
        if (entity.Buckets.Any()) throw new Exception($"Cannot delete a BucketGroup with Buckets assigned to it.");

        DatabaseContext.BucketGroup.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var cleansedEntities = ids
            .Where(i => i != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToList();
        var entities = DatabaseContext.BucketGroup
            .Include(i => i.Buckets)
            .Where(i => cleansedEntities.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No BucketGroup found with passed IDs.");
        if (entities.Any(i => i.Buckets.Any())) throw new Exception($"Cannot delete a BucketGroup with Buckets assigned to it.");

        DatabaseContext.BucketGroup.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}
