using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BudgetedTransactionRepository : IBudgetedTransactionRepository
{
    private DatabaseContext DatabaseContext { get; }
    
    public BudgetedTransactionRepository(DatabaseContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }
    
    public IQueryable<BudgetedTransaction> All() => DatabaseContext.BudgetedTransaction
        .AsNoTracking();
    
    public IQueryable<BudgetedTransaction> AllWithIncludedEntities() => DatabaseContext.BudgetedTransaction
        .Include(i => i.Bucket)
        .Include(i => i.Transaction)
        .AsNoTracking();
    
    public IQueryable<BudgetedTransaction> AllWithTransactions() => DatabaseContext.BudgetedTransaction
        .Include(i => i.Transaction)
        .Include(i => i.Transaction.Account)
        .AsNoTracking();

    public BudgetedTransaction? ById(Guid id) => DatabaseContext.BudgetedTransaction
        .FirstOrDefault(i => i.Id == id);
    
    public BudgetedTransaction? ByIdWithTransaction(Guid id) => DatabaseContext.BudgetedTransaction
        .Include(i => i.Transaction)
        .Include(i => i.Transaction.Account)
        .FirstOrDefault(i => i.Id == id);
    
    public BudgetedTransaction? ByIdWithIncludedEntities(Guid id) => DatabaseContext.BudgetedTransaction
        .Include(i => i.Bucket)
        .Include(i => i.Transaction)
        .FirstOrDefault(i => i.Id == id);

    public int Create(BudgetedTransaction entity)
    {
        DatabaseContext.BudgetedTransaction.Add(entity);
        return DatabaseContext.SaveChanges();
    }

    public int CreateRange(IEnumerable<BudgetedTransaction> entities)
    {
        DatabaseContext.BudgetedTransaction.AddRange(entities);
        return DatabaseContext.SaveChanges();
    }

    public int Update(BudgetedTransaction entity)
    {
        // DatabaseContext.BudgetedTransaction.Update(entity);
        // return DatabaseContext.SaveChanges();
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    public int UpdateRange(IEnumerable<BudgetedTransaction> entities)
    {
        // DatabaseContext.BudgetedTransaction.UpdateRange(entities);
        // return DatabaseContext.SaveChanges();
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    public int Delete(Guid id)
    {
        var entity = DatabaseContext.BudgetedTransaction.FirstOrDefault(i => i.Id == id);
        if (entity == null) throw new Exception($"BudgetedTransaction with id {id} not found.");

        DatabaseContext.BudgetedTransaction.Remove(entity);
        return DatabaseContext.SaveChanges();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var entities = DatabaseContext.BudgetedTransaction.Where(i => ids.Contains(i.Id));
        if (!entities.Any()) throw new Exception($"No BudgetedTransactions found with passed IDs.");

        DatabaseContext.BudgetedTransaction.RemoveRange(entities);
        return DatabaseContext.SaveChanges();
    }
}