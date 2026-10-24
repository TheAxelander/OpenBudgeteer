using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericAccountService<TDatabase> : GenericBaseService<Account, TDatabase>, IAccountService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericAccountService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }

    protected abstract override IAccountRepository CreateBaseRepository(TDatabase dbConnection);
    protected abstract IBankTransactionRepository CreateBankTransactionRepository(TDatabase dbConnection);

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

    public virtual IEnumerable<Account> GetActiveAccounts()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var accountRepository = CreateBaseRepository(dbConnection);
            return accountRepository
                .All()
                .Where(i => i.IsActive == 1)
                .OrderBy(i => i.Name)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }
    
    /// <summary>
    /// Sets Inactive flag for a record in the database based on <see cref="Account"/> id.
    /// </summary>
    /// <returns>Response containing details and success of the request</returns>
    public virtual Account CloseAccount(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBankTransactionRepository(dbConnection);
            var balance = bankTransactionRepository
                .All()
                .Where(i => i.AccountId == id)
                .ToList()
                .Sum(i => i.Amount);
            
            if (balance != 0) throw new EntityUpdateException("Balance must be 0 to close an Account");
        
            var account = Get(id);
            account.IsActive = 0;
            return Update(account);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to close Account: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}