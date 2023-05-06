using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.ViewModels;

public class RecurringTransactionViewModel : ViewModelBase
{
    private RecurringTransactionViewModelItem _newTransaction;
    /// <summary>
    /// Helper property to handle creation of a new <see cref="RecurringTransactionViewModelItem"/>
    /// </summary>
    public RecurringTransactionViewModelItem NewTransaction
    {
        get => _newTransaction;
        set => Set(ref _newTransaction, value);
    }
    
    private ObservableCollection<RecurringTransactionViewModelItem> _transactions;
    /// <summary>
    /// Collection of loaded Recurring Transactions
    /// </summary>
    public ObservableCollection<RecurringTransactionViewModelItem> Transactions
    {
        get => _transactions;
        protected set => Set(ref _transactions, value);
    }
    
    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    
    /// <summary>
    /// Basic Constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public RecurringTransactionViewModel(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
        _transactions = new ObservableCollection<RecurringTransactionViewModelItem>();
    }
    
    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync()
    {
        try
        {
            _transactions.Clear();

            using var dbContext = new DatabaseContext(_dbOptions);
            var transactionTasks = new List<Task<RecurringTransactionViewModelItem>>();

            foreach (var transaction in dbContext.RecurringBankTransaction)
            {
                transactionTasks.Add(RecurringTransactionViewModelItem.CreateAsync(_dbOptions, transaction));
            }

            foreach (var transaction in await Task.WhenAll(transactionTasks))
            {
                _transactions.Add(transaction);
            }

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Creates a new <see cref="RecurringTransactionViewModelItem"/> which can be modified directly
    /// </summary>
    public void AddEmptyTransaction()
    {
        var newTransaction = new RecurringTransactionViewModelItem(_dbOptions);
        newTransaction.InModification = true;
        _transactions.Insert(0, newTransaction);
    }

    /// <summary>
    /// Helper method to start modification process for all Transactions based on current Filter
    /// </summary>
    public void EditAllTransaction()
    {
        foreach (var transaction in Transactions)
        {
            transaction.StartModification();
        }
    }

    /// <summary>
    /// Starts update process for all Transactions
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveAllTransaction()
    {
        try
        {
            foreach (var transaction in _transactions.Where(i => i.InModification))
            {
                var result = transaction.UpdateItem();
                if (!result.IsSuccessful) throw new Exception(result.Message);
            }
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Cancels update process for all Transactions. Reloads ViewModel to restore data.
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> CancelAllTransactionAsync()
    {
        return await LoadDataAsync();
    }
}