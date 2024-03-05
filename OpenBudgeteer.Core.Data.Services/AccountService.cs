using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class AccountService : BaseService<Account>, IAccountService
{
    internal AccountService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new AccountRepository(new DatabaseContext(dbContextOptions)))
    {
    }

    public override Account Get(Guid id)
    {
        var result = base.Get(id);
        if (result.IsActive == 0) result.Name += " (Inactive)";
        return result;
    }

    public override IEnumerable<Account> GetAll()
    {
        var result = base.GetAll().ToList();
        foreach (var account in result.Where(account => account.IsActive == 0))
        {
            account.Name += " (Inactive)";
        }

        return result;
    }

    public IEnumerable<Account> GetActiveAccounts()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new AccountRepository(dbContext);
            return repository.All()
                .Where(i => i.IsActive == 1)
                .OrderBy(i => i.Name)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
    
    /// <summary>
    /// Sets Inactive flag for a record in the database based on <see cref="Account"/> id.
    /// </summary>
    /// <returns>Response containing details and success of the request</returns>
    public Account CloseAccount(Guid id)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        var repository = new BankTransactionRepository(dbContext);
        var balance = repository.All()
            .Where(i => i.AccountId.ToString() == id.ToString())
            .ToList()
            .Sum(i => i.Amount);
        if (balance != 0) throw new Exception("Balance must be 0 to close an Account");
        
        var account = Get(id);
        account.IsActive = 0;
        return Update(account);
    }
}