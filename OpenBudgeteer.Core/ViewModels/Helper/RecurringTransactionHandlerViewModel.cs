using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.Helper;

public class RecurringTransactionHandlerViewModel : ViewModelBase
{
    private RecurringTransactionViewModel? _newRecurringTransaction;
    /// <summary>
    /// Helper property to handle creation of a new <see cref="RecurringBankTransaction"/>
    /// </summary>
    public RecurringTransactionViewModel? NewRecurringTransaction
    {
        get => _newRecurringTransaction;
        set => Set(ref _newRecurringTransaction, value);
    }
    
    private ObservableCollection<RecurringTransactionViewModel> _transactions;
    /// <summary>
    /// Collection of loaded Recurring Transactions
    /// </summary>
    public ObservableCollection<RecurringTransactionViewModel> Transactions
    {
        get => _transactions;
        protected set => Set(ref _transactions, value);
    }
    
    /// <summary>
    /// Basic Constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public RecurringTransactionHandlerViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        _transactions = new ObservableCollection<RecurringTransactionViewModel>();
        ResetNewTransaction();
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

            var availableAccounts = ServiceManager.AccountService
                .GetActiveAccounts()
                .Select(i => AccountViewModel.CreateFromAccount(ServiceManager, i))
                .ToList();

            var transactionTasks = ServiceManager.RecurringBankTransactionService
                .GetAllWithEntities()
                .Select(transaction => RecurringTransactionViewModel
                    .CreateFromRecurringTransactionAsync(ServiceManager, availableAccounts, transaction))
                .ToList();

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
    /// Starts creation process based on <see cref="NewRecurringTransaction"/>
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateItem()
    {
        try
        {
            if (_newRecurringTransaction is null) throw new Exception("New Recurring Transaction has not been initialized");
            var result = _newRecurringTransaction.CreateOrUpdateTransaction();
            if (!result.IsSuccessful) return result;
            ResetNewTransaction();
    
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
    
    /// <summary>
    /// Helper method to reset values of <see cref="NewRecurringTransaction"/>
    /// </summary>
    public void ResetNewTransaction()
    {
        // Use previous entered date
        var lastEnteredDate = _newRecurringTransaction?.FirstOccurrenceDate ?? DateOnly.FromDateTime(DateTime.Today);
        
        _newRecurringTransaction = RecurringTransactionViewModel.CreateEmpty(ServiceManager);
        _newRecurringTransaction.FirstOccurrenceDate = lastEnteredDate;
    }
    
    /// <summary>
    /// Creates a new <see cref="RecurringTransactionViewModel"/> which can be modified directly
    /// </summary>
    public void AddEmptyTransaction()
    {
        var newTransaction = RecurringTransactionViewModel.CreateEmpty(ServiceManager);
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
                var result = transaction.CreateOrUpdateTransaction();
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