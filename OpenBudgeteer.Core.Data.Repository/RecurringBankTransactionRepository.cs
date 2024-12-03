using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class RecurringBankTransactionRepository : IRecurringBankTransactionRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public RecurringBankTransactionRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<RecurringBankTransaction> All() => DatabaseContext.RecurringBankTransaction
        .AsNoTracking();
    
    public IQueryable<RecurringBankTransaction> AllWithIncludedEntities() => DatabaseContext.RecurringBankTransaction
        .Include(i => i.Account)
        .AsNoTracking();

    public RecurringBankTransaction? ById(Guid id) => DatabaseContext.RecurringBankTransaction
        .FirstOrDefault(i => i.Id == id);
    
    public RecurringBankTransaction? ByIdWithIncludedEntities(Guid id) => DatabaseContext.RecurringBankTransaction
        .Include(i => i.Account)
        .FirstOrDefault(i => i.Id == id);

    public int Create(RecurringBankTransaction entity)
    {
        DatabaseContext.RecurringBankTransaction.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        DatabaseContext.RecurringBankTransaction.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(RecurringBankTransaction entity)
    {
        DatabaseContext.RecurringBankTransaction.Update(entity);
        return DatabaseContext.SaveChanges();
    }

    public int UpdateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        DatabaseContext.RecurringBankTransaction.UpdateRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.RecurringBankTransaction.FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"RecurringBankTransaction with id {id} not found.");

        DatabaseContext.RecurringBankTransaction.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.RecurringBankTransaction.Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No RecurringBankTransactions found with passed IDs.");

        DatabaseContext.RecurringBankTransaction.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}