using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketRepository : IBucketRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BucketRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<Bucket> All() => DatabaseContext.Bucket
        .AsNoTracking();
    
    public IQueryable<Bucket> AllWithVersions() => DatabaseContext.Bucket
        .Include(i => i.BucketVersions)
        .AsNoTracking();
    
    public IQueryable<Bucket> AllWithActivities() => DatabaseContext.Bucket
        .Include(i => i.BucketMovements)
        .Include(i => i.BudgetedTransactions).ThenInclude(i => i.Transaction)
        .AsNoTracking();
    
    public IQueryable<Bucket> AllWithIncludedEntities() => DatabaseContext.Bucket
        .Include(i => i.BucketGroup)
        .Include(i => i.BucketMovements)
        .Include(i => i.BucketVersions)
        .Include(i => i.BudgetedTransactions)
        .AsNoTracking();
    
    public Bucket? ById(Guid id) => DatabaseContext.Bucket
        .FirstOrDefault(i => i.Id == id);
    
    public Bucket? ByIdWithVersions(Guid id) => DatabaseContext.Bucket
        .Include(i => i.BucketVersions)
        .FirstOrDefault(i => i.Id == id);
    
    public Bucket? ByIdWithMovements(Guid id) => DatabaseContext.Bucket
        .Include(i => i.BucketMovements)
        .FirstOrDefault(i => i.Id == id);
    
    public Bucket? ByIdWithTransactions(Guid id) => DatabaseContext.Bucket
        .Include(i => i.BudgetedTransactions).ThenInclude(i => i.Transaction)
        .FirstOrDefault(i => i.Id == id);
    
    /*public Bucket? ByIdWithActivities(Guid id) => DatabaseContext.Bucket
        .Include(i => i.BucketMovements)
        .Include(i => i.BudgetedTransactions).ThenInclude(i => i.Transaction)
        .FirstOrDefault(i => i.Id == id);*/
    
    public Bucket? ByIdWithIncludedEntities(Guid id) => DatabaseContext.Bucket
        .Include(i => i.BucketGroup)
        .Include(i => i.BucketMovements)
        .Include(i => i.BucketVersions)
        .Include(i => i.BudgetedTransactions)
        .FirstOrDefault(i => i.Id == id);

    public int Create(Bucket entity)
    {
        DatabaseContext.Bucket.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<Bucket> entities)
    {
        DatabaseContext.Bucket.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(Bucket entity)
    {
        DatabaseContext.Bucket.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<Bucket> entities)
    {
        DatabaseContext.Bucket.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        if (id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
            id == Guid.Parse("00000000-0000-0000-0000-000000000002")) return 0;
        
        var entity = DatabaseContext.Bucket
            .Include(i => i.BucketMovements)
            .Include(i => i.BucketVersions)
            .Include(i => i.BudgetedTransactions)
            .FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"Bucket with id {id} not found.");
        if (entity.BucketMovements.Any()) throw new Exception($"Cannot delete a Bucket with BucketMovements assigned to it.");
        if (entity.BudgetedTransactions.Any()) throw new Exception($"Cannot delete a Bucket with BudgetedTransactions assigned to it.");

        DatabaseContext.Bucket.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var cleansedEntities = ids
            .Where(i => 
                i != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();
        
        var entities = DatabaseContext.Bucket
            .Include(i => i.BucketMovements)
            .Include(i => i.BucketVersions)
            .Include(i => i.BudgetedTransactions)
            .Where(i => cleansedEntities.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No Buckets found with passed IDs.");
        if (entities.Any(i => i.BucketMovements.Any())) throw new Exception($"Cannot delete a Bucket with BucketMovements assigned to it.");
        if (entities.Any(i => i.BudgetedTransactions.Any())) throw new Exception($"Cannot delete a Bucket with BudgetedTransactions assigned to it.");

        DatabaseContext.Bucket.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}