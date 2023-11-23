using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.Helper;

public class RecurringTransactionHandlerViewModel : ViewModelBase
{
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
                .GetAll()
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