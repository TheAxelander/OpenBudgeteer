using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BankTransactionRepository : IBankTransactionRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BankTransactionRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IEnumerable<BankTransaction> All() => DatabaseContext.BankTransaction
        .AsNoTracking();
    
    public IEnumerable<BankTransaction> AllWithIncludedEntities() => DatabaseContext.BankTransaction
        .Include(i => i.Account)
        .AsNoTracking();

    public BankTransaction? ById(Guid id) => DatabaseContext.BankTransaction
        .FirstOrDefault(i => i.Id == id);
    
    public BankTransaction? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BankTransaction
        .Include(i => i.Account)
        .Include(i => i.BudgetedTransactions)
        .FirstOrDefault(i => i.Id == id);

    public int Create(BankTransaction entity)
    {
        DatabaseContext.BankTransaction.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BankTransaction> entities)
    {
        DatabaseContext.BankTransaction.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BankTransaction entity)
    {
        DatabaseContext.BankTransaction.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<BankTransaction> entities)
    {
        DatabaseContext.BankTransaction.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.BankTransaction
            .Include(i => i.BudgetedTransactions)
            .FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"BankTransaction with id {id} not found.");

        DatabaseContext.BankTransaction.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.BankTransaction
            .Include(i => i.BudgetedTransactions)
            .Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No BankTransactions found with passed IDs.");

        DatabaseContext.BankTransaction.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}