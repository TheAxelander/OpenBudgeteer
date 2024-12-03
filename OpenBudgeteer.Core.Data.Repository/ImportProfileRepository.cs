using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class ImportProfileRepository : IImportProfileRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public ImportProfileRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<ImportProfile> All() => DatabaseContext.ImportProfile
        .AsNoTracking();
    
    public IQueryable<ImportProfile> AllWithIncludedEntities() => DatabaseContext.ImportProfile
        .Include(i => i.Account)
        .AsNoTracking();

    public ImportProfile? ById(Guid id) => DatabaseContext.ImportProfile
        .FirstOrDefault(i => i.Id == id);
    
    public ImportProfile? ByIdWithIncludedEntities(Guid id) => DatabaseContext.ImportProfile
        .Include(i => i.Account)
        .FirstOrDefault(i => i.Id == id);

    public int Create(ImportProfile entity)
    {
        DatabaseContext.ImportProfile.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<ImportProfile> entities)
    {
        DatabaseContext.ImportProfile.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(ImportProfile entity)
    {
        DatabaseContext.ImportProfile.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<ImportProfile> entities)
    {
        DatabaseContext.ImportProfile.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.ImportProfile.FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"ImportProfile with id {id} not found.");

        DatabaseContext.ImportProfile.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.ImportProfile.Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No ImportProfiles found with passed IDs.");

        DatabaseContext.ImportProfile.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}