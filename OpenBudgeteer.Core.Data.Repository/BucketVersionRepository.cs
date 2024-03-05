using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketVersionRepository : IBucketVersionRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BucketVersionRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<BucketVersion> All() => DatabaseContext.BucketVersion
        .AsNoTracking();
    
    public IQueryable<BucketVersion> AllWithIncludedEntities() => DatabaseContext.BucketVersion
        .Include(i => i.Bucket)
        .AsNoTracking();

    public BucketVersion? ById(Guid id) => DatabaseContext.BucketVersion
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());
    
    public BucketVersion? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BucketVersion
        .Include(i => i.Bucket)
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());

    public int Create(BucketVersion entity)
    {
        DatabaseContext.BucketVersion.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BucketVersion> entities)
    {
        DatabaseContext.BucketVersion.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BucketVersion entity)
    {
        DatabaseContext.BucketVersion.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<BucketVersion> entities)
    {
        DatabaseContext.BucketVersion.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.BucketVersion.FirstOrDefault(i => i.Id.ToString() == id.ToString());
        if (entity == null) throw new Exception($"BucketVersion with id {id} not found.");

        DatabaseContext.BucketVersion.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.BucketVersion.Where(i => ids.Select(j => j.ToString()).Contains(i.Id.ToString()));
        if (!entities.Any()) throw new Exception($"No BucketVersions found with passed IDs.");

        DatabaseContext.BucketVersion.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}