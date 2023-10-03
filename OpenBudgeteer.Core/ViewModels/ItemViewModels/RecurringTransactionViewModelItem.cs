using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Contracts.Models;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class RecurringTransactionViewModelItem : ViewModelBase
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
    
    private ObservableCollection<Account> _availableAccounts;
    /// <summary>
    /// Helper collection to list all existing Account
    /// </summary>
    public ObservableCollection<Account> AvailableAccounts
    {
        get => _availableAccounts;
        set => Set(ref _availableAccounts, value);
    }

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
    
    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private RecurringTransactionViewModelItem _oldRecurringTransactionViewModelItem;

    /// <summary>
    /// Basic constructor
    /// </summary>
    public RecurringTransactionViewModelItem()
    {
        Transaction = new RecurringBankTransaction();
        AvailableAccounts = new ObservableCollection<Account>();
    }

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public RecurringTransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions) : this()
    {
        _dbOptions = dbOptions;
        
        // Set initial FirstOccurenceDate in case of "Create new Recurring Transaction"
        Transaction.FirstOccurrenceDate = DateTime.Today;
        Transaction.RecurrenceType = 1;
        SelectedRecurrenceType = AvailableRecurrenceTypes.First();
        
        // Get all available Accounts for ComboBox selections
        // Add empty Account for empty pre-selection
        AvailableAccounts.Add(new Account
        {
            AccountId = Guid.Empty,
            IsActive = 1,
            Name = "No Account"
        });
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            foreach (var account in dbContext.Account.Where(i => i.IsActive == 1))
            {
                AvailableAccounts.Add(account);
            }
        }            
        SelectedAccount = AvailableAccounts.First();
    }
    
    /// <summary>
    /// Initialize ViewModel with an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="transaction">Transaction instance</param>
    public RecurringTransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, 
        RecurringBankTransaction transaction) : this(dbOptions)
    {
        // Make a copy of the object to prevent any double Bindings
        Transaction = new RecurringBankTransaction()
        {
            TransactionId = transaction.TransactionId,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Memo = transaction.Memo,
            Payee = transaction.Payee,
            FirstOccurrenceDate = transaction.FirstOccurrenceDate,
            RecurrenceType = transaction.RecurrenceType,
            RecurrenceAmount = transaction.RecurrenceAmount
        };
        // Pre-selection the right account
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var account = dbContext.Account.First(i => i.AccountId == transaction.AccountId);
            if (account is { IsActive: 0 })
            {
                account.Name += " (Inactive)";
                AvailableAccounts.Add(account);
            }
            SelectedAccount = account;
        }
        // Pre-select empty Account if no Account was found (for new RecurringTransaction())
        SelectedAccount ??= AvailableAccounts.First();
        
        // Pre-selection the right RecurrenceType
        // or Pre-select RecurrenceType if no RecurrenceType was found (for new RecurringTransaction())
        SelectedRecurrenceType = AvailableRecurrenceTypes.FirstOrDefault(i => 
                i.Key == transaction.RecurrenceType, AvailableRecurrenceTypes.First());
    }

    /// <summary>
    /// Initialize ViewModel with an existing <see cref="BankTransaction"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="transaction">Transaction instance</param>
    public RecurringTransactionViewModelItem(DbContextOptions<DatabaseContext> dbOptions, BankTransaction transaction) 
        : this(dbOptions,
        new RecurringBankTransaction()
        {
            TransactionId = Guid.Empty,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Memo = transaction.Memo,
            Payee = transaction.Payee,
            FirstOccurrenceDate = transaction.TransactionDate
        })
    { }
    
    /// <summary>
    /// Initialize and return a new ViewModel based on an existing <see cref="RecurringBankTransaction"/> object
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="transaction">Transaction instance</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<RecurringTransactionViewModelItem> CreateAsync(DbContextOptions<DatabaseContext> dbOptions, 
        RecurringBankTransaction transaction)
    {
        return await Task.Run(() => new RecurringTransactionViewModelItem(dbOptions, transaction));
    }
    
    /// <summary>
    /// Creates or updates a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult CreateOrUpdateTransaction()
    {
        var result = PerformConsistencyCheck();
        if (!result.IsSuccessful) return result;

        using var dbContext = new DatabaseContext(_dbOptions);
        try
        {
            var transactionId = Transaction.TransactionId;
            Transaction.AccountId = SelectedAccount.AccountId;
            Transaction.RecurrenceType = SelectedRecurrenceType.Key;

            if (transactionId != Guid.Empty)
            {
                // Update RecurringBankTransaction in DB
                dbContext.UpdateRecurringBankTransaction(Transaction);
            }
            else
            {
                // Create RecurringBankTransaction in DB
                if (dbContext.CreateRecurringBankTransaction(Transaction) == 0)
                    throw new Exception("Recurring Transaction could not be created in database.");
            }

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
        if (Transaction == null) return new ViewModelOperationResult(false, "Errors in Transaction object.");
        if (SelectedAccount == null || SelectedAccount.AccountId == Guid.Empty) return new ViewModelOperationResult(false, "No Bank account selected.");
        
        return new ViewModelOperationResult(true);
    }
    
    /// <summary>
    /// Removes a record in the database based on <see cref="Transaction"/> object
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult DeleteTransaction()
    {
        using var dbContext = new DatabaseContext(_dbOptions);
        try
        {
            // Delete RecurringBankTransaction in DB
            dbContext.DeleteRecurringBankTransaction(Transaction);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    public void StartModification()
    {
        _oldRecurringTransactionViewModelItem = new RecurringTransactionViewModelItem(_dbOptions, Transaction);
        InModification = true;
    }

    public void CancelModification()
    {
        Transaction = _oldRecurringTransactionViewModelItem.Transaction;
        SelectedAccount = _oldRecurringTransactionViewModelItem.SelectedAccount;
        SelectedRecurrenceType = _oldRecurringTransactionViewModelItem.SelectedRecurrenceType;
        InModification = false;
        _oldRecurringTransactionViewModelItem = null;
    }
    
    public ViewModelOperationResult CreateItem()
    {
        Transaction.TransactionId = Guid.Empty; // Triggers CREATE during CreateOrUpdateTransaction()
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
        if (Transaction.TransactionId == Guid.Empty) 
            return new ViewModelOperationResult(false, "Transaction needs to be created first in database");

        var result = CreateOrUpdateTransaction();
        if (!result.IsSuccessful)
        {
            return new ViewModelOperationResult(false, result.Message, true);
        }
        _oldRecurringTransactionViewModelItem = null;
        InModification = false;

        return new ViewModelOperationResult(true, false);
    }

    public ViewModelOperationResult DeleteItem()
    {
        var result = DeleteTransaction();
        return result.IsSuccessful ? new ViewModelOperationResult(true, true) : result;
    }
}