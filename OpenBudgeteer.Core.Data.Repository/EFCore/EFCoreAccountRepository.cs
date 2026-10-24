using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.EFCore;

public class EFCoreAccountRepository : IAccountRepository
{
    private DatabaseContext DatabaseContext { get; }

    public EFCoreAccountRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    public IQueryable<Account> All() => DatabaseContext.Account
        .AsNoTracking();

    public IQueryable<Account> AllWithIncludedEntities() => DatabaseContext.Account
        .AsNoTracking();

    public Account? ById(Guid id) => DatabaseContext.Account
        .FirstOrDefault(i => i.Id == id);

    public Account? ByIdWithIncludedEntities(Guid id) => DatabaseContext.Account
        .FirstOrDefault(i => i.Id == id);

    public int Create(Account entity)
    {
        DatabaseContext.Account.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<Account> entities)
    {
        DatabaseContext.Account.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(Account entity)
    {
        DatabaseContext.Account.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<Account> entities)
    {
        DatabaseContext.Account.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = ById(id);
        if (entity is null) throw new Exception($"Account with id {id} not found.");

        DatabaseContext.Account.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = ids
            .Select(ById)
            .OfType<Account>()
            .ToList();
        if (entities.Count == 0) throw new Exception($"No Account found with passed IDs.");

        DatabaseContext.Account.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}
