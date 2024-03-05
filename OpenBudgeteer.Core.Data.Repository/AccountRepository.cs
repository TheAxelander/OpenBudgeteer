using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class AccountRepository : IAccountRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public AccountRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    public IQueryable<Account> All() => DatabaseContext.Account
        .AsNoTracking();
    
    public IQueryable<Account> AllWithIncludedEntities() => DatabaseContext.Account
        .AsNoTracking();

    public Account? ById(Guid id) => DatabaseContext.Account
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());
    
    public Account? ByIdWithIncludedEntities(Guid id) => DatabaseContext.Account
        .FirstOrDefault(i => i.Id.ToString() == id.ToString());

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
        var entity = DatabaseContext.Account.FirstOrDefault(i => i.Id.ToString() == id.ToString());
        if (entity == null) throw new Exception($"Account with id {id} not found.");

        DatabaseContext.Account.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.Account.Where(i => ids.Select(j => j.ToString()).Contains(i.Id.ToString()));
        if (!entities.Any()) throw new Exception($"No Account found with passed IDs.");

        DatabaseContext.Account.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}