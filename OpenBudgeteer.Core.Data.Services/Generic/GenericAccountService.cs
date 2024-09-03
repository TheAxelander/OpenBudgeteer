using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericAccountService : GenericBaseService<Account>, IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IBankTransactionRepository _bankTransactionRepository;
    
    public GenericAccountService(
        IAccountRepository accountRepository, 
        IBankTransactionRepository bankTransactionRepository) : base(accountRepository)
    {
        _accountRepository = accountRepository;
        _bankTransactionRepository = bankTransactionRepository;
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
        return _accountRepository
            .All()
            .Where(i => i.IsActive == 1)
            .OrderBy(i => i.Name)
            .ToList();
    }
    
    /// <summary>
    /// Sets Inactive flag for a record in the database based on <see cref="Account"/> id.
    /// </summary>
    /// <returns>Response containing details and success of the request</returns>
    public Account CloseAccount(Guid id)
    {
        var balance = _bankTransactionRepository
            .All()
            .Where(i => i.AccountId == id)
            .ToList()
            .Sum(i => i.Amount);
            
        if (balance != 0) throw new Exception("Balance must be 0 to close an Account");
        
        var account = Get(id);
        account.IsActive = 0;
        return Update(account);
    }
}