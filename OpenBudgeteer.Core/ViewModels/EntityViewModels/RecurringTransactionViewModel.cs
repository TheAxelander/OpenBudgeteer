using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class RecurringTransactionViewModel : BaseEntityViewModel<RecurringBankTransaction>
{
    #region Properties & Fields
    
    public enum RecurringTransactionRecurrenceType
    {
        Days = 1,
        Months = 2,
        Quarter = 3,
        Years = 4
    }
    
    /// <summary>
    /// Database Id of the RecurringBankTransaction
    /// </summary>
    public readonly Guid RecurringTransactionId;
    
    private Guid _accountId;
    /// <summary>
    /// Database Id of the Account assigned to this RecurringBankTransaction
    /// </summary>
    public Guid AccountId 
    { 
        get => _accountId;
        set => Set(ref _accountId, value);
    }

    private RecurringTransactionRecurrenceType _recurrenceType;
    /// <summary>
    /// Recurrence Type that is selected for the Transaction
    /// </summary>
    public RecurringTransactionRecurrenceType RecurrenceType 
    { 
        get => _recurrenceType;
        set => Set(ref _recurrenceType, value);
    }

    /// <summary>
    /// Output of <see cref="RecurrenceType"/> used for display purposes
    /// </summary>
    public string RecurrenceTypeOutput
    {
        get
        {
            return RecurrenceType switch
            {
                RecurringTransactionRecurrenceType.Days => nameof(RecurringTransactionRecurrenceType.Days),
                RecurringTransactionRecurrenceType.Months => nameof(RecurringTransactionRecurrenceType.Months),
                RecurringTransactionRecurrenceType.Quarter => nameof(RecurringTransactionRecurrenceType.Quarter),
                RecurringTransactionRecurrenceType.Years => nameof(RecurringTransactionRecurrenceType.Years),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    private int _recurrenceAmount;
    /// <summary>
    /// How often the Transaction repeats in combination with <see cref="RecurrenceType"/>
    /// </summary>
    public int RecurrenceAmount 
    { 
        get => _recurrenceAmount;
        set => Set(ref _recurrenceAmount, value);
    }
    
    private DateTime _firstOccurrenceDate;
    /// <summary>
    /// Date on which the series starts
    /// </summary>
    public DateTime FirstOccurrenceDate 
    { 
        get => _firstOccurrenceDate;
        set => Set(ref _firstOccurrenceDate, value);
    }
    
    private string _payee;
    /// <summary>
    /// Value for <see cref="BankTransaction.Payee"/>
    /// </summary>
    public string Payee 
    { 
        get => _payee;
        set => Set(ref _payee, value);
    }
    
    private string _memo;
    /// <summary>
    /// Value for <see cref="BankTransaction.Memo"/>
    /// </summary>
    public string Memo 
    { 
        get => _memo;
        set => Set(ref _memo, value);
    }
    
    private decimal _amount;
    /// <summary>
    /// Value for <see cref="BankTransaction.Amount"/>
    /// </summary>
    public decimal Amount 
    { 
        get => _amount;
        set => Set(ref _amount, value);
    }
    
    private AccountViewModel _selectedAccount;
    /// <summary>
    /// Account where the Transaction is assigned to
    /// </summary>
    public AccountViewModel SelectedAccount
    {
        get => _selectedAccount;
        set => Set(ref _selectedAccount, value);
    }
    
    private bool _inModification;
    /// <summary>
    /// Helper property to check if the Transaction is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        set => Set(ref _inModification, value);
    }

    private bool _isHovered;
    /// <summary>
    /// Helper property to check if the cursor hovers over the entry in the UI
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set => Set(ref _isHovered, value);
    }
    
    /// <summary>
    /// Helper collection to list all existing Account
    /// </summary>
    public readonly ObservableCollection<AccountViewModel> AvailableAccounts;

    /// <summary>
    /// Helper collection to list all existing Recurrence Types
    /// </summary>
    public List<KeyValuePair<int, string>> AvailableRecurrenceTypes => new()
    {
        new KeyValuePair<int, string>(1, "Weeks"),
        new KeyValuePair<int, string>(2, "Months"),
        new KeyValuePair<int, string>(3, "Quarters"),
        new KeyValuePair<int, string>(4, "Years")
    };
    
    private RecurringTransactionViewModel? _oldRecurringTransactionViewModelItem;
    
    #endregion
    
    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    protected RecurringTransactionViewModel(IServiceManager serviceManager, IEnumerable<AccountViewModel> availableAccounts, 
        RecurringBankTransaction? transaction) : base(serviceManager)
    {
        // Handle Accounts
        AvailableAccounts = new ObservableCollection<AccountViewModel>();
        foreach (var availableAccount in availableAccounts)
        {
            AvailableAccounts.Add(availableAccount);
        }
        
        // Handle Transaction
        if (transaction == null)
        {
            // Add the "No Account" for pre-selection
            var noAccount = new Account
            {
                Id = Guid.Empty,
                IsActive = 1,
                Name = "No Account"
            };
            _selectedAccount = AccountViewModel.CreateFromAccount(serviceManager, noAccount);
            AvailableAccounts.Add(_selectedAccount);
            
            _firstOccurrenceDate = DateTime.Today;
            _recurrenceType = RecurringTransactionRecurrenceType.Days;
            _payee = string.Empty;
            _memo = string.Empty;
        }
        else
        {
            RecurringTransactionId = transaction.Id;
            _accountId = transaction.AccountId;
            _amount = transaction.Amount;
            _memo = transaction.Memo ?? string.Empty;
            _payee = transaction.Payee ?? string.Empty;
            _firstOccurrenceDate = transaction.FirstOccurrenceDate;
            _recurrenceType = (RecurringTransactionRecurrenceType)transaction.RecurrenceType;
            _recurrenceAmount = transaction.RecurrenceAmount;
            
            // Make inactive Account available in Selection, will later fail during saving
            _selectedAccount = AccountViewModel.CreateFromAccount(serviceManager, transaction.Account);
            if (SelectedAccount.IsActive) AvailableAccounts.Add(_selectedAccount);
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected RecurringTransactionViewModel(RecurringTransactionViewModel viewModel) : base(viewModel.ServiceManager)
    {
        // Handle Accounts
        AvailableAccounts = new ObservableCollection<AccountViewModel>();
        foreach (var availableAccount in viewModel.AvailableAccounts)
        {
            AvailableAccounts.Add(availableAccount);
        }
        
        // Handle Transaction
        RecurringTransactionId = viewModel.RecurringTransactionId;
        _accountId = viewModel.AccountId;
        _recurrenceType = viewModel.RecurrenceType;
        _recurrenceAmount = viewModel.RecurrenceAmount;
        _firstOccurrenceDate = viewModel.FirstOccurrenceDate;
        _payee = viewModel.Payee;
        _memo = viewModel.Memo;
        _amount = viewModel.Amount;
        _selectedAccount = viewModel.SelectedAccount;
    }
    
    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <returns>New ViewModel instance</returns>
    public static RecurringTransactionViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var availableAccounts = serviceManager.AccountService
            .GetActiveAccounts()
            .Select(i => AccountViewModel.CreateFromAccount(serviceManager, i))
            .ToList();
        return new RecurringTransactionViewModel(serviceManager, availableAccounts, null);
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<RecurringTransactionViewModel> CreateFromRecurringTransactionAsync(
        IServiceManager serviceManager, 
        IEnumerable<AccountViewModel> availableAccounts,
        RecurringBankTransaction transaction)
    {
        return await Task.Run(() => CreateFromRecurringTransaction(serviceManager, availableAccounts, transaction));
    }
    
    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static RecurringTransactionViewModel CreateFromRecurringTransaction(
        IServiceManager serviceManager, 
        IEnumerable<AccountViewModel> availableAccounts,
        RecurringBankTransaction transaction)
    {
        return new RecurringTransactionViewModel(serviceManager, availableAccounts, transaction);
    }
    
    /// <summary>
    /// Initialize ViewModel converting a <see cref="BankTransaction"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction which will be used for conversion</param>
    public static async Task<RecurringTransactionViewModel> CreateFromBankTransactionAsync(
        IServiceManager serviceManager,
        IEnumerable<AccountViewModel> availableAccounts,
        BankTransaction transaction)
    {
        var newRecurringTransaction = new RecurringBankTransaction()
        {
            Id = Guid.Empty,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Memo = transaction.Memo ?? string.Empty,
            Payee = transaction.Payee ?? string.Empty,
            FirstOccurrenceDate = transaction.TransactionDate
        };
        return await CreateFromRecurringTransactionAsync(serviceManager, availableAccounts, newRecurringTransaction);
    }
    
    #endregion
    
    #region Modification Handler
    
    internal override RecurringBankTransaction ConvertToDto()
    {
        return new RecurringBankTransaction()
        {
            Id = RecurringTransactionId,
            AccountId = AccountId,
            RecurrenceType = (int)RecurrenceType,
            RecurrenceAmount = RecurrenceAmount,
            Payee = Payee,
            Memo = Memo,
            Amount = Amount,
            FirstOccurrenceDate = FirstOccurrenceDate
        };
    }
    
    /// <summary>
    /// Creates or updates a record in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateOrUpdateTransaction()
    {
        var result = PerformConsistencyCheck();
        if (!result.IsSuccessful) return result;

        try
        {
            if (RecurringTransactionId == Guid.Empty)
                ServiceManager.RecurringBankTransactionService.Create(ConvertToDto());
            else
                ServiceManager.RecurringBankTransactionService.Update(ConvertToDto());

            _oldRecurringTransactionViewModelItem = null;
            InModification = false;
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    /// <summary>
    /// Executes several data consistency checks to see if changes can be stored in the database 
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult PerformConsistencyCheck()
    {
        // Consistency and Validity Checks
        if (RecurrenceAmount == 0) return new ViewModelOperationResult(false, "Invalid Recurrence details.");
        //if (Transaction == null) return new ViewModelOperationResult(false, "Errors in Transaction object.");
        if (SelectedAccount.AccountId == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        if (!SelectedAccount.IsActive) return new ViewModelOperationResult(false, "The selected Bank account is inactive.");
        
        return new ViewModelOperationResult(true);
    }
    
    /// <summary>
    /// Removes a record in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult DeleteTransaction()
    {
        try
        {
            // Delete RecurringBankTransaction in DB
            ServiceManager.RecurringBankTransactionService.Delete(RecurringTransactionId);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    /// <summary>
    /// Updates properties to set the ViewModel into Modification state
    /// </summary>
    public void StartModification()
    {
        _oldRecurringTransactionViewModelItem = new RecurringTransactionViewModel(this);
        InModification = true;
    }

    /// <summary>
    /// Updates properties to set the ViewModel into Default state and restoring original values
    /// </summary>
    public void CancelModification()
    {
        if (_oldRecurringTransactionViewModelItem != null)
        {
            AccountId = _oldRecurringTransactionViewModelItem.AccountId;
            RecurrenceType = _oldRecurringTransactionViewModelItem.RecurrenceType;
            RecurrenceAmount = _oldRecurringTransactionViewModelItem.RecurrenceAmount;
            Payee = _oldRecurringTransactionViewModelItem.Payee;
            Memo = _oldRecurringTransactionViewModelItem.Memo;
            Amount = _oldRecurringTransactionViewModelItem.Amount;
            FirstOccurrenceDate = _oldRecurringTransactionViewModelItem.FirstOccurrenceDate;
            SelectedAccount = _oldRecurringTransactionViewModelItem.SelectedAccount;
        }
        InModification = false;
        _oldRecurringTransactionViewModelItem = null;
    }

    /// <summary>
    /// Removes a record in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteItem()
    {
        var result = DeleteTransaction();
        return result.IsSuccessful ? new ViewModelOperationResult(true, true) : result;
    }
    
    #endregion
}