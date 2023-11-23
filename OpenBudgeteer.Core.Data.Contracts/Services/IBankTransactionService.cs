using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBankTransactionService : IBaseService<BankTransaction>
{
    public BankTransaction GetWithEntities(Guid id);
    public IEnumerable<BankTransaction> GetAll(DateTime? periodStart, DateTime? periodEnd, int limit = 0);
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, int limit = 0);
    public IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateTime? periodStart, DateTime? periodEnd, int limit = 0);
    public IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities);
}