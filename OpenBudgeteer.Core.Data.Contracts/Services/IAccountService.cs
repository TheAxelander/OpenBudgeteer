using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IAccountService : IBaseService<Account>
{
    public IEnumerable<Account> GetActiveAccounts();
    public Account CloseAccount(Guid id);
}