using System;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class AccountViewModelItem : ViewModelBase
{
    private Account _account;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public Account Account
    {
        get => _account;
        internal set => Set(ref _account, value);
    }

    private decimal _balance;
    /// <summary>
    /// Total account balance
    /// </summary>
    public decimal Balance
    {
        get => _balance;
        set => Set(ref _balance, value);
    }

    private decimal _in;
    /// <summary>
    /// Total income of the account
    /// </summary>
    public decimal In
    {
        get => _in;
        set => Set(ref _in, value);
    }

    private decimal _out;
    /// <summary>
    /// Total expenses of the account
    /// </summary>
    public decimal Out
    {
        get => _out;
        set => Set(ref _out, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public AccountViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    /// <summary>
    /// Initialize ViewModel with an existing <see cref="Account"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="account">Account instance</param>
    public AccountViewModelItem(DbContextOptions<DatabaseContext> dbOptions, Account account) : this(dbOptions)
    {
        _account = account;
    }

    /// <summary>
    /// Creates or updates a record in the database based on <see cref="Account"/> object
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateUpdateAccount()
    {
        using var dbContext = new DatabaseContext(_dbOptions);
        var result = Account.AccountId == Guid.Empty ? dbContext.CreateAccount(Account) : dbContext.UpdateAccount(Account);
        return result == 0 
            ? new ViewModelOperationResult(false, "Unable to save changes to database") 
            : new ViewModelOperationResult(true, true);
    }

    /// <summary>
    /// Sets Inactive flag for a record in the database based on <see cref="Account"/> object.
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseAccount()
    {
        if (Balance != 0) return new ViewModelOperationResult(false, "Balance must be 0 to close an Account");

        Account.IsActive = 0;
        using var dbContext = new DatabaseContext(_dbOptions);
        var result = dbContext.UpdateAccount(Account);
        return result == 0 
            ? new ViewModelOperationResult(false, "Unable to save changes to database") 
            : new ViewModelOperationResult(true, true);
    }
}
