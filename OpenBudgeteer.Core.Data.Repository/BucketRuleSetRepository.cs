using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BucketRuleSetRepository : IBucketRuleSetRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BucketRuleSetRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<BucketRuleSet> All() => DatabaseContext.BucketRuleSet
        .AsNoTracking();
    
    public IQueryable<BucketRuleSet> AllWithIncludedEntities() => DatabaseContext.BucketRuleSet
        .Include(i => i.TargetBucket)
        .Include(i => i.MappingRules)
        .AsNoTracking();

    public BucketRuleSet? ById(Guid id) => DatabaseContext.BucketRuleSet
        .FirstOrDefault(i => i.Id == id);
    
    public BucketRuleSet? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BucketRuleSet
        .Include(i => i.TargetBucket)
        .Include(i => i.MappingRules)
        .FirstOrDefault(i => i.Id == id);

    public int Create(BucketRuleSet entity)
    {
        DatabaseContext.BucketRuleSet.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BucketRuleSet> entities)
    {
        DatabaseContext.BucketRuleSet.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BucketRuleSet entity)
    {
        DatabaseContext.BucketRuleSet.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<BucketRuleSet> entities)
    {
        DatabaseContext.BucketRuleSet.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.BucketRuleSet
            .Include(i => i.MappingRules)
            .FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"BucketRuleSet with id {id} not found.");

        DatabaseContext.BucketRuleSet.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.BucketRuleSet
            .Include(i => i.MappingRules)
            .Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No BucketRuleSets found with passed IDs.");

        DatabaseContext.BucketRuleSet.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}