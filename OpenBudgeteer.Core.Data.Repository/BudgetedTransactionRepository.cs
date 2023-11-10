using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BudgetedTransactionRepository : BaseRepository<BudgetedTransaction>, IBudgetedTransactionRepository
{
    public BudgetedTransactionRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
   
    public override IQueryable<BudgetedTransaction> All() => DatabaseContext
        .Set<BudgetedTransaction>()
        .Include(i => i.Transaction)
        .AsNoTracking();

    public override IQueryable<BudgetedTransaction> Where(Expression<Func<BudgetedTransaction, bool>> expression) 
        => DatabaseContext
            .Set<BudgetedTransaction>()
            .Include(i => i.Transaction)
            .Where(expression)
            .AsNoTracking();
    
    public override BudgetedTransaction? ById(Guid id) => DatabaseContext
        .Set<BudgetedTransaction>()
        .Include(i => i.Transaction)
        .FirstOrDefault(i => i.Id == id);
}