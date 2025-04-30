using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBankTransactionService : IBaseService<BankTransaction>
{
    public BankTransaction GetWithEntities(Guid id);
    public IEnumerable<BankTransaction> GetAll(DateOnly? periodStart, DateOnly? periodEnd, int limit = 0);
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0);
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateOnly? periodStart, DateOnly? periodEnd, int limit = 0);
    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities);
}