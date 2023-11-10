using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class RecurringBankTransactionRepository : BaseRepository<RecurringBankTransaction>, IRecurringBankTransactionRepository
{
    public RecurringBankTransactionRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
    
    public override IQueryable<RecurringBankTransaction> All() => DatabaseContext
        .Set<RecurringBankTransaction>()
        .Include(i => i.Account)
        .AsNoTracking();

    public override IQueryable<RecurringBankTransaction> Where(Expression<Func<RecurringBankTransaction, bool>> expression) 
        => DatabaseContext
            .Set<RecurringBankTransaction>()
            .Include(i => i.Account)
            .Where(expression)
            .AsNoTracking();
    
    public override RecurringBankTransaction? ById(Guid id) => DatabaseContext
        .Set<RecurringBankTransaction>()
        .Include(i => i.Account)
        .FirstOrDefault(i => i.Id == id);
}