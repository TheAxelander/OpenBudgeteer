using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class RecurringTransactionViewModel : ViewModelBase
{
    private RecurringBankTransaction _transaction;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public RecurringBankTransaction Transaction
    {
        get => _transaction;
        internal set => Set(ref _transaction, value);
    }
    
    private Account _selectedAccount;
    /// <summary>
    /// Account where the Transaction is assigned to
    /// </summary>
    public Account SelectedAccount
    {
        get => _selectedAccount;
        set => Set(ref _selectedAccount, value);
    }
    
    private KeyValuePair<int, string> _selectedRecurrenceType;
    /// <summary>
    /// Recurrence Type that is selected for the Transaction
    /// </summary>
    public KeyValuePair<int, string> SelectedRecurrenceType
    {
        get => _selectedRecurrenceType;
        set => Set(ref _selectedRecurrenceType, value);
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
    public readonly ObservableCollection<Account> AvailableAccounts;

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

    /// <summary>
    /// Initialize ViewModel with an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableAccounts">List of all available <see cref="Account"/> from database. (Use a cached list here)</param>
    /// <param name="transaction">Transaction instance</param>
    protected RecurringTransactionViewModel(IServiceManager serviceManager, IEnumerable<Account> availableAccounts, 
        RecurringBankTransaction? transaction) : base(serviceManager)
    {
        // Handle Accounts
        AvailableAccounts = new ObservableCollection<Account>();
        foreach (var availableAccount in availableAccounts)
        {
            AvailableAccounts.Add(availableAccount);
        }
        
        // Handle Transaction
        if (transaction == null)
        {
            _transaction = new RecurringBankTransaction()
            {
                FirstOccurrenceDate = DateTime.Today,
                RecurrenceType = 1
            };
            
            // Add the "No Account" for pre-selection
            var noAccount = new Account
            {
                Id = Guid.Empty,
                IsActive = 1,
                Name = "No Account"
            };
            AvailableAccounts.Add(noAccount);
            _selectedAccount = noAccount;
        }
        else
        {
            // Make a copy of the object to prevent any double Bindings
            _transaction = new RecurringBankTransaction()
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                Memo = transaction.Memo,
                Payee = transaction.Payee,
                FirstOccurrenceDate = transaction.FirstOccurrenceDate,
                RecurrenceType = transaction.RecurrenceType,
                RecurrenceAmount = transaction.RecurrenceAmount
            };
            
            // Make inactive Account available in Selection, will later fail during saving
            _selectedAccount = transaction.Account;
            if (SelectedAccount is { IsActive: 0 }) AvailableAccounts.Add(_selectedAccount);
            
            // Pre-selection the right RecurrenceType
            // or set default RecurrenceType if nothing was found (for new RecurringTransaction())
            SelectedRecurrenceType = AvailableRecurrenceTypes.FirstOrDefault(i => 
                    i.Key == transaction.RecurrenceType, AvailableRecurrenceTypes.First());
        }
    }
    
    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <returns>New ViewModel instance</returns>
    public static RecurringTransactionViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var availableAccounts = serviceManager.AccountService.GetActiveAccounts().ToList();
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
        IEnumerable<Account> availableAccounts,
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
        IEnumerable<Account> availableAccounts,
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
        IEnumerable<Account> availableAccounts,
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
    
    /// <summary>
    /// Creates or updates a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult CreateOrUpdateTransaction()
    {
        var result = PerformConsistencyCheck();
        if (!result.IsSuccessful) return result;

        try
        {
            var transactionId = Transaction.Id;
            Transaction.AccountId = SelectedAccount.Id;
            Transaction.RecurrenceType = SelectedRecurrenceType.Key;

            if (transactionId == Guid.Empty)
                ServiceManager.RecurringBankTransactionService.Create(Transaction);
            else
                ServiceManager.RecurringBankTransactionService.Update(Transaction);

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
        if (Transaction.RecurrenceAmount == 0) return new ViewModelOperationResult(false, "Invalid Recurrence details.");
        //if (Transaction == null) return new ViewModelOperationResult(false, "Errors in Transaction object.");
        if (SelectedAccount.Id == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        if (SelectedAccount.IsActive == 0) return new ViewModelOperationResult(false, "The selected Bank account is inactive.");
        
        return new ViewModelOperationResult(true);
    }
    
    /// <summary>
    /// Removes a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult DeleteTransaction()
    {
        try
        {
            // Delete RecurringBankTransaction in DB
            ServiceManager.RecurringBankTransactionService.Delete(Transaction);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    public void StartModification()
    {
        _oldRecurringTransactionViewModelItem = CreateFromRecurringTransaction(ServiceManager, AvailableAccounts, Transaction);
        InModification = true;
    }

    public void CancelModification()
    {
        Transaction = _oldRecurringTransactionViewModelItem!.Transaction;
        SelectedAccount = _oldRecurringTransactionViewModelItem.SelectedAccount;
        SelectedRecurrenceType = _oldRecurringTransactionViewModelItem.SelectedRecurrenceType;
        InModification = false;
        _oldRecurringTransactionViewModelItem = null;
    }
    
    public ViewModelOperationResult CreateItem()
    {
        Transaction.Id = Guid.Empty; // Triggers CREATE during CreateOrUpdateTransaction()
        var result = CreateOrUpdateTransaction();
        // Keep invalid Item active in Modification mode
        if (result.IsSuccessful)
        {
            InModification = false;
        }
        return result;
    }

    public ViewModelOperationResult UpdateItem()
    {
        if (Transaction.Id == Guid.Empty) 
            return new ViewModelOperationResult(false, "Transaction needs to be created first in database");

        var result = CreateOrUpdateTransaction();
        if (!result.IsSuccessful)
        {
            return new ViewModelOperationResult(false, result.Message, true);
        }
        _oldRecurringTransactionViewModelItem = null;
        InModification = false;

        return new ViewModelOperationResult(true);
    }

    public ViewModelOperationResult DeleteItem()
    {
        var result = DeleteTransaction();
        return result.IsSuccessful ? new ViewModelOperationResult(true, true) : result;
    }
}