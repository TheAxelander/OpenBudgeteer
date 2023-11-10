using System;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class AccountViewModel : ViewModelBase
{
    private Account _account;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public Account Account
    {
        get => _account;
        private set => Set(ref _account, value);
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
    
    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Account"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="account">Account instance</param>
    protected AccountViewModel(IServiceManager serviceManager, Account? account) : base(serviceManager)
    {
        _account = account ?? new Account()
        {
            Id = Guid.Empty,
            Name = "New Account", 
            IsActive = 1
        };
    }

    /// <summary>
    /// Initialize ViewModel for creating a new <see cref="Account"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public static AccountViewModel CreateEmpty(IServiceManager serviceManager)
    {
        return new AccountViewModel(serviceManager, null);
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Account"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="account">Account instance</param>
    public static AccountViewModel CreateFromAccount(IServiceManager serviceManager, Account account)
    {
        return new AccountViewModel(serviceManager, account);
    }
    
    /// <summary>
    /// Creates or updates a record in the database based on <see cref="AccountViewModel"/> object
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateUpdateAccount()
    {
        try
        {
            if (Account.Id == Guid.Empty)
                ServiceManager.AccountService.Create(Account);
            else
                ServiceManager.AccountService.Update(Account);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
    
    /// <summary>
    /// Sets Inactive flag for a record in the database based on <see cref="AccountViewModel"/> object.
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseAccount()
    {
        try
        {
            ServiceManager.AccountService.CloseAccount(Account);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message); 
        }
    }
}