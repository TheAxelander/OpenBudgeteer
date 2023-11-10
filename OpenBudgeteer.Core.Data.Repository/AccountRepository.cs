using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(DatabaseContext databaseContext) : base(databaseContext)
    {
    }
}