using System;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class AccountViewModel : BaseEntityViewModel<Account>
{
    #region Properties & Fields

    /// <summary>
    /// Database Id of the Account
    /// </summary>
    public readonly Guid AccountId;
    
    private string _name;
    /// <summary>
    /// Name of the Account
    /// </summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }
    
    private bool _isActive;
    /// <summary>
    /// Active status of the Account
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        private set => Set(ref _isActive, value);
    }

    private decimal _balance;
    /// <summary>
    /// Total Account balance
    /// </summary>
    public decimal Balance
    {
        get => _balance;
        set => Set(ref _balance, value);
    }

    private decimal _in;
    /// <summary>
    /// Total income of the Account
    /// </summary>
    public decimal In
    {
        get => _in;
        set => Set(ref _in, value);
    }

    private decimal _out;
    /// <summary>
    /// Total expenses of the Account
    /// </summary>
    public decimal Out
    {
        get => _out;
        set => Set(ref _out, value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Account"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="account">Account instance</param>
    protected AccountViewModel(IServiceManager serviceManager, Account? account) : base(serviceManager)
    {
        if (account == null)
        {
            AccountId = Guid.Empty;
            _name = "New Account";
            _isActive = true;
        }
        else
        {
            AccountId = account.Id;
            _name = account.Name ?? string.Empty;
            _isActive = account.IsActive != 0;
            if (!IsActive) _name += " (Inactive)";
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected AccountViewModel(AccountViewModel viewModel) : base(viewModel.ServiceManager)
    {
        AccountId = viewModel.AccountId;
        _name = viewModel.Name;
        _isActive = viewModel.IsActive;
        _balance = viewModel.Balance;
        _in = viewModel.In;
        _out = viewModel.Out;
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
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new AccountViewModel(this);
    }

    #endregion

    #region Modification Handler

    /// <summary>
    /// Convert current ViewModel into a corresponding <see cref="IEntity"/> object
    /// </summary>
    /// <returns>Converted ViewModel</returns>
    internal override Account ConvertToDto()
    {
        return new Account()
        {
            Id = AccountId,
            Name = Name,
            IsActive = IsActive ? 1 : 0
        };
    }

    /// <summary>
    /// Creates or updates a record in the database based on ViewModel data
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateOrUpdateAccount()
    {
        try
        {
            if (AccountId == Guid.Empty)
                ServiceManager.AccountService.Create(ConvertToDto());
            else
                ServiceManager.AccountService.Update(ConvertToDto());
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
    
    /// <summary>
    /// Sets Inactive flag for a record in the database based on ViewModel data
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseAccount()
    {
        try
        {
            ServiceManager.AccountService.CloseAccount(AccountId);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message); 
        }
    }
    
    #endregion
}