using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class TransactionPageViewModel : TransactionListingViewModel
{
    private TransactionViewModel? _newTransaction;
    /// <summary>
    /// Helper property to handle creation of a new <see cref="BankTransaction"/>
    /// </summary>
    public TransactionViewModel? NewTransaction
    {
        get => _newTransaction;
        set => Set(ref _newTransaction, value);
    }
    
    private int _proposeBucketsCount;
    /// <summary>
    /// Helper property for Progress Dialog during Bucket proposal process
    /// </summary>
    public int ProposeBucketsCount
    {
        get => _proposeBucketsCount;
        set => Set(ref _proposeBucketsCount, value);
    }

    private int _proposeBucketsProgress;
    /// <summary>
    /// Helper property for Progress Dialog during Bucket proposal process
    /// </summary>
    public int ProposeBucketsProgress
    {
        get => _proposeBucketsProgress;
        set => Set(ref _proposeBucketsProgress, value);
    }

    private int _proposeBucketsPercentage;
    /// <summary>
    /// Helper property for Progress Dialog during Bucket proposal process
    /// </summary>
    public int ProposeBucketsPercentage
    {
        get => _proposeBucketsPercentage;
        set => Set(ref _proposeBucketsPercentage, value);
    }
    
    private TransactionFilter _currentFilter;
    /// <summary>
    /// Sets the current filter for the ViewModel
    /// </summary>
    public TransactionFilter CurrentFilter
    {
        get => _currentFilter;
        set
        {
            if (Set(ref _currentFilter, value))
                NotifyPropertyChanged(nameof(Transactions));
        } 
    }

    /// <summary>
    /// Collection of loaded Transactions
    /// </summary>
    public override ObservableCollection<TransactionViewModel> Transactions
    {
        get
        {
            switch (CurrentFilter)
            {
                case TransactionFilter.HideMapped:
                    return new ObservableCollection<TransactionViewModel>(
                        _transactions.Where(i => 
                            i.Buckets.First().SelectedBucket.Id == Guid.Empty ||
                            i.InModification));
                case TransactionFilter.OnlyMapped:
                    return new ObservableCollection<TransactionViewModel>(
                        _transactions.Where(i => 
                            i.Buckets.First().SelectedBucket.Id != Guid.Empty ||
                            i.InModification));
                case TransactionFilter.InModification:
                    return new ObservableCollection<TransactionViewModel>(
                        _transactions.Where(i => i.InModification));
                case TransactionFilter.NoFilter:
                default:
                    return _transactions;
            }
        }
        protected set => Set(ref _transactions, value);
    }
    
    /// <summary>
    /// EventHandler which should be invoked during Bucket Proposal to track overall Progress
    /// </summary>
    public event EventHandler<ProposeBucketChangedEventArgs>? BucketProposalProgressChanged;

    private readonly YearMonthSelectorViewModel _yearMonthViewModel;
    
    /// <summary>
    /// Basic Constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
    public TransactionPageViewModel(IServiceManager serviceManager, YearMonthSelectorViewModel yearMonthViewModel) 
        : base(serviceManager, yearMonthViewModel)
    {
        _yearMonthViewModel = yearMonthViewModel;
        _transactions = new ObservableCollection<TransactionViewModel>();
        ResetNewTransaction();
        //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
    }
    
    /// <summary>
    /// Starts creation process based on <see cref="NewTransaction"/>
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateItem()
    {
        try
        {
            if (NewTransaction == null) throw new Exception("New Transaction has not been initialized");
            NewTransaction.Transaction.Id = Guid.Empty;
            var result = NewTransaction.CreateItem();
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
    /// Helper method to reset values of <see cref="NewTransaction"/>
    /// </summary>
    public void ResetNewTransaction()
    {
        var lastEnteredDate = NewTransaction == null ?
            _yearMonthViewModel.CurrentMonth :
            NewTransaction.Transaction.TransactionDate;
        NewTransaction = TransactionViewModel.CreateEmpty(ServiceManager);
        NewTransaction.Transaction.TransactionDate = lastEnteredDate;
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

        CurrentFilter = TransactionFilter.InModification;
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
            CurrentFilter = TransactionFilter.NoFilter;
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
        CurrentFilter = TransactionFilter.NoFilter;
        return await LoadDataAsync();
    }
    
    /// <summary>
    /// Starts process to propose the right <see cref="Bucket"/> for all Transactions
    /// </summary>
    /// <remarks>Sets all Transactions into Modification Mode in case they have a "No Selection" Bucket</remarks>
    public async Task ProposeBuckets()
    {
        CurrentFilter = TransactionFilter.InModification;
        var unassignedTransactions = _transactions
            .Where(i => i.Buckets.First().SelectedBucket.Id == Guid.Empty)
            .ToList();
        ProposeBucketsCount = unassignedTransactions.Count;
        ProposeBucketsProgress = 0;

        var proposalTasks = unassignedTransactions.Select(transaction => 
            Task.Run(() =>
            {
                transaction.StartModification();
                transaction.ProposeBucket();
                ProposeBucketsProgress++;
                ProposeBucketsPercentage = ProposeBucketsCount == 0 
                    ? 0 
                    : Convert.ToInt32(decimal.Divide(ProposeBucketsProgress, ProposeBucketsCount) * 100);
                BucketProposalProgressChanged?.Invoke(this, 
                    new ProposeBucketChangedEventArgs(ProposeBucketsProgress, ProposeBucketsPercentage));
            }));

        await Task.WhenAll(proposalTasks);
    }

    /// <summary>
    /// Iterates all existing <see cref="RecurringBankTransaction"/> and checks if they need to be created in
    /// current month.
    /// </summary>
    /// <remarks>There is no check on already existing <see cref="BankTransaction"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> AddRecurringTransactionsAsync()
    {
        try
        {
            var result = await ServiceManager.RecurringBankTransactionService
                .CreatePendingBankTransactionAsync(_yearMonthViewModel.CurrentMonth);
            return result.Any() 
                ? new ViewModelOperationResult(true, true)
                : new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
}