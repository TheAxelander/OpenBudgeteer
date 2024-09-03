using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Repositories;

public interface IBudgetedTransactionRepository : IBaseRepository<BudgetedTransaction>
{
    public IEnumerable<BudgetedTransaction> AllWithTransactions();
    public BudgetedTransaction? ByIdWithTransaction(Guid id);
}