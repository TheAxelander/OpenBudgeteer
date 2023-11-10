using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class BankTransactionRepository : BaseRepository<BankTransaction>, IBankTransactionRepository
{
    public BankTransactionRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
    
    public override IQueryable<BankTransaction> All() => DatabaseContext
        .Set<BankTransaction>()
        .Include(i => i.Account)
        .AsNoTracking();

    public override IQueryable<BankTransaction> Where(Expression<Func<BankTransaction, bool>> expression) 
        => DatabaseContext
            .Set<BankTransaction>()
            .Include(i => i.Account)
            .Where(expression)
            .AsNoTracking();
    
    public override BankTransaction? ById(Guid id) => DatabaseContext
        .Set<BankTransaction>()
        .Include(i => i.Account)
        .FirstOrDefault(i => i.Id == id);
}