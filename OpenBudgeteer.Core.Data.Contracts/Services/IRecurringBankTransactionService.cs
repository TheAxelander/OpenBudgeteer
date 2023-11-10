using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IRecurringBankTransactionService : IBaseService<RecurringBankTransaction>
{
    public Task<IEnumerable<BankTransaction>> GetPendingBankTransactionAsync(DateTime yearMonth);
    public Task<IEnumerable<BankTransaction>> CreatePendingBankTransactionAsync(DateTime yearMonth);
}