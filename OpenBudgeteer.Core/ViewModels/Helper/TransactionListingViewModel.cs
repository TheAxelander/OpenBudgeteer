using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.Helper;

public class TransactionListingViewModel : ViewModelBase
{
    protected ObservableCollection<TransactionViewModel> _transactions;
    /// <summary>
    /// Collection of loaded Transactions
    /// </summary>
    public virtual ObservableCollection<TransactionViewModel> Transactions
    {
        get => _transactions;
        protected set => Set(ref _transactions, value);
    }
    
    private readonly YearMonthSelectorViewModel _yearMonthViewModel;

    public TransactionListingViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        _transactions = new ObservableCollection<TransactionViewModel>();
        _yearMonthViewModel = new YearMonthSelectorViewModel(serviceManager);
    }
    
    public TransactionListingViewModel(IServiceManager serviceManager, YearMonthSelectorViewModel yearMonthViewModel) : this(serviceManager)
    {
        _yearMonthViewModel = yearMonthViewModel;
    }
    
    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync()
    {
        try
        {
            // Get all available transactions. The TransactionViewModelItem takes care to find all assigned buckets for 
            // each passed transaction. It creates also the respective ViewModelObjects
            _transactions.Clear();
            
            var availableAccounts = ServiceManager.AccountService.GetActiveAccounts().ToList();
            var availableBuckets = ServiceManager.BucketService.GetActiveBuckets(_yearMonthViewModel.CurrentMonth).ToList();
            var transactionTasks = ServiceManager.BankTransactionService
                .GetAll(
                    _yearMonthViewModel.CurrentPeriod.Item1,
                    _yearMonthViewModel.CurrentPeriod.Item2)
                .Select(i => TransactionViewModel
                    .CreateFromTransactionAsync(ServiceManager, availableAccounts, availableBuckets, i))
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
    /// Initialize ViewModel and load data from database but only for <see cref="BankTransaction"/> assigned to the
    /// passed <see cref="Bucket"/>. Optionally <see cref="BucketMovement"/> will be transformed to <see cref="BankTransaction"/>
    /// </summary>
    /// <param name="bucket">Bucket for which Transactions should be loaded</param>
    /// <param name="withMovements">Include <see cref="BucketMovement"/> which will be transformed to <see cref="BankTransaction"/></param>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync(Bucket bucket, bool withMovements)
    {
        try
        {
            _transactions.Clear();

            // Get all BankTransaction
            var transactionTasks = ServiceManager.BudgetedTransactionService.GetAllFromBucket(bucket.Id)
                .Select(i => 
                    TransactionViewModel.CreateFromTransactionWithoutBucketsAsync(ServiceManager, i.Transaction))
                .ToList();

            if (withMovements)
            {
                // Get Bucket Movements
                transactionTasks.AddRange(ServiceManager.BucketMovementService.GetAllFromBucket(bucket.Id)
                    .Select(i => 
                        TransactionViewModel.CreateFromBucketMovementAsync(ServiceManager, i)));
            }

            foreach (var transaction in (await Task.WhenAll(transactionTasks))
                     .OrderByDescending(i => i.Transaction.TransactionDate))
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
    /// Initialize ViewModel and load data from database but only for <see cref="BankTransaction"/> assigned to the
    /// passed <see cref="Account"/>
    /// </summary>
    /// <param name="account">Account for which Transactions should be loaded</param>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync(Account account)
    {
        try
        {
            _transactions.Clear();
            var transactionTasks = ServiceManager.BankTransactionService.GetFromAccount(account.Id)
                .Select(i => 
                    TransactionViewModel.CreateFromTransactionWithoutBucketsAsync(ServiceManager, i));

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
}