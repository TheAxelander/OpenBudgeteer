using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class TransactionPageViewModel : TransactionListingViewModel
{
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
    
    /// <summary>
    /// Collection of loaded Transactions
    /// </summary>
    public override ObservableCollection<TransactionViewModel> Transactions
    {
        get => _transactions;
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
    public TransactionPageViewModel(
        IServiceManager serviceManager, 
        YearMonthSelectorViewModel yearMonthViewModel) 
        : base(serviceManager, yearMonthViewModel)
    {
        _yearMonthViewModel = yearMonthViewModel;
        _transactions = new ObservableCollection<TransactionViewModel>();
        //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
    }
    
    /// <summary>
    /// Creates the passed <see cref="TransactionViewModel"/> in the database
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateItem(TransactionViewModel transaction)
    {
        try
        {
            var result = transaction.CreateOrUpdateTransaction();
            return !result.IsSuccessful ? result : new ViewModelOperationResult(true, true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
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
                if (!result.IsSuccessful) return new ViewModelOperationResult(false, result.Message);
            }
            return new ViewModelOperationResult(true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }

    /// <summary>
    /// Starts process to propose the right <see cref="Bucket"/> for all Transactions
    /// </summary>
    /// <remarks>Sets all Transactions into Modification Mode in case they have a "No Selection" Bucket</remarks>
    public async Task ProposeBuckets()
    {
        var unassignedTransactions = _transactions
            .Where(i => i.Buckets.First().SelectedBucketId == Guid.Empty)
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
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }
}