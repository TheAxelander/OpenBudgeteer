using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class MappingRuleRepository : IMappingRuleRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public MappingRuleRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IEnumerable<MappingRule> All() => DatabaseContext.MappingRule
        .AsNoTracking();
    
    public IEnumerable<MappingRule> AllWithIncludedEntities() => DatabaseContext.MappingRule
        .Include(i => i.BucketRuleSet)
        .AsNoTracking();

    public MappingRule? ById(Guid id) => DatabaseContext.MappingRule
        .FirstOrDefault(i => i.Id == id);
    
    public MappingRule? ByIdWithIncludedEntities(Guid id) => DatabaseContext.MappingRule
        .Include(i => i.BucketRuleSet)
        .FirstOrDefault(i => i.Id == id);

    public int Create(MappingRule entity)
    {
        DatabaseContext.MappingRule.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<MappingRule> entities)
    {
        DatabaseContext.MappingRule.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(MappingRule entity)
    {
        DatabaseContext.MappingRule.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<MappingRule> entities)
    {
        DatabaseContext.MappingRule.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.MappingRule.FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"MappingRule with id {id} not found.");

        DatabaseContext.MappingRule.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.MappingRule.Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No MappingRules found with passed IDs.");

        DatabaseContext.MappingRule.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}