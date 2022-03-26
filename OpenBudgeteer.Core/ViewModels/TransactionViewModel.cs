using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;
using OpenBudgeteer.Core.Common.Extensions;

namespace OpenBudgeteer.Core.ViewModels;

/// <summary>
/// Identifier which kind of filter can be applied on the <see cref="TransactionViewModel"/> 
/// </summary>
public enum TransactionViewModelFilter: int
{
    [StringValue("No Filter")]
    NoFilter = 0, 
    [StringValue("Hide mapped")]
    HideMapped = 1, 
    [StringValue("Only mapped")]
    OnlyMapped = 2,
    [StringValue("In Modification")]
    InModification = 3,
}

public class TransactionViewModel : ViewModelBase
{
    private TransactionViewModelItem _newTransaction;
    /// <summary>
    /// Helper property to handle creation of a new <see cref="BankTransaction"/>
    /// </summary>
    public TransactionViewModelItem NewTransaction
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
    
    private TransactionViewModelFilter _currentFilter;
    /// <summary>
    /// Sets the current filter for the ViewModel
    /// </summary>
    public TransactionViewModelFilter CurrentFilter
    {
        get => _currentFilter;
        set
        {
            if (Set(ref _currentFilter, value))
                NotifyPropertyChanged(nameof(Transactions));
        } 
    }

    private ObservableCollection<TransactionViewModelItem> _transactions;
    /// <summary>
    /// Collection of loaded Transactions
    /// </summary>
    public ObservableCollection<TransactionViewModelItem> Transactions
    {
        get
        {
            switch (CurrentFilter)
            {
                case TransactionViewModelFilter.HideMapped:
                    return new ObservableCollection<TransactionViewModelItem>(
                        _transactions.Where(i => 
                            i.Buckets.First().SelectedBucket.BucketId == 0 ||
                            i.InModification));
                case TransactionViewModelFilter.OnlyMapped:
                    return new ObservableCollection<TransactionViewModelItem>(
                        _transactions.Where(i => 
                            i.Buckets.First().SelectedBucket.BucketId > 0 ||
                            i.InModification));
                case TransactionViewModelFilter.InModification:
                    return new ObservableCollection<TransactionViewModelItem>(
                        _transactions.Where(i => i.InModification));
                case TransactionViewModelFilter.NoFilter:
                default:
                    return _transactions;
            }
        }
        protected set => Set(ref _transactions, value);
    }
    
    /// <summary>
    /// EventHandler which should be invoked during Bucket Proposal to track overall Progress
    /// </summary>
    public event EventHandler<ProposeBucketChangedEventArgs> BucketProposalProgressChanged;

    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private readonly YearMonthSelectorViewModel _yearMonthViewModel;

    /// <summary>
    /// Basic Constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
    public TransactionViewModel(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel)
    {
        _dbOptions = dbOptions;
        _yearMonthViewModel = yearMonthViewModel;
        ResetNewTransaction();
        _transactions = new ObservableCollection<TransactionViewModelItem>();
        //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
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

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var sql = $"SELECT * FROM {nameof(BankTransaction)} " +
                      $"WHERE {nameof(BankTransaction.TransactionDate)} LIKE '{_yearMonthViewModel.CurrentMonth:yyyy-MM}%' " +
                      $"ORDER BY {nameof(BankTransaction.TransactionDate)}";
                var transactions = dbContext.BankTransaction.FromSqlRaw(sql);

                var transactionTasks = new List<Task<TransactionViewModelItem>>();

                foreach (var transaction in transactions)
                {
                    transactionTasks.Add(TransactionViewModelItem.CreateAsync(_dbOptions, _yearMonthViewModel, transaction));
                }

                foreach (var transaction in await Task.WhenAll(transactionTasks))
                {
                    _transactions.Add(transaction);
                }

                return new ViewModelOperationResult(true);
            }
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

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var transactionTasks = new List<Task<TransactionViewModelItem>>();

                // Get all BankTransaction
                var results = dbContext.BankTransaction
                    .Join(
                        dbContext.BudgetedTransaction,
                        bankTransaction => bankTransaction.TransactionId,
                        budgetedTransaction => budgetedTransaction.TransactionId,
                        (bankTransaction, budgetedTransaction) => new
                        {
                            BankTransaction = bankTransaction,
                            BudgetedTransaction = budgetedTransaction
                        })
                    .Where(i => i.BudgetedTransaction.BucketId == bucket.BucketId)
                    .OrderByDescending(i => i.BankTransaction.TransactionDate)
                    .ToList();

                foreach (var result in results)
                {
                    transactionTasks.Add(TransactionViewModelItem.CreateWithoutBucketsAsync(_dbOptions, _yearMonthViewModel, result.BankTransaction));
                }

                if (withMovements)
                {
                    // Get Bucket Movements
                    var bucketMovements = dbContext.BucketMovement
                            .Where(i => i.BucketId == bucket.BucketId)
                            .ToList();
                    foreach (var bucketMovement in bucketMovements)
                    {
                        transactionTasks.Add(TransactionViewModelItem.CreateFromBucketMovementAsync(bucketMovement));
                    }
                }

                foreach (var transaction in (await Task.WhenAll(transactionTasks))
                    .OrderByDescending(i => i.Transaction.TransactionDate))
                {
                    _transactions.Add(transaction);
                }

                return new ViewModelOperationResult(true);
            }
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
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var results = 
                    dbContext.BankTransaction
                        .Where(i => i.AccountId == account.AccountId)
                        .OrderByDescending(i => i.TransactionDate)
                        .ToList();

                var transactions = results.Count < 100 ? results : results.GetRange(0, 100);
                var transactionTasks = new List<Task<TransactionViewModelItem>>();
                foreach (var transaction in transactions)
                {
                    transactionTasks.Add(TransactionViewModelItem.CreateWithoutBucketsAsync(_dbOptions, _yearMonthViewModel, transaction));
                }

                foreach (var transaction in await Task.WhenAll(transactionTasks))
                {
                    _transactions.Add(transaction);
                }

                return new ViewModelOperationResult(true);
            }
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Starts creation process based on <see cref="NewTransaction"/>
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateItem()
    {
        NewTransaction.Transaction.TransactionId = 0;
        var result = NewTransaction.CreateItem();
        if (!result.IsSuccessful) return result;
        ResetNewTransaction();
        
        return new ViewModelOperationResult(true, true);
    }

    /// <summary>
    /// Helper method to reset values of <see cref="NewTransaction"/>
    /// </summary>
    public void ResetNewTransaction()
    {
        NewTransaction = new TransactionViewModelItem(_dbOptions, _yearMonthViewModel, true);
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

        CurrentFilter = TransactionViewModelFilter.InModification;
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
            CurrentFilter = TransactionViewModelFilter.NoFilter;
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
        CurrentFilter = TransactionViewModelFilter.NoFilter;
        return await LoadDataAsync();
    }

    /// <summary>
    /// Starts process to propose the right <see cref="Bucket"/> for all Transactions
    /// </summary>
    /// <remarks>Sets all Transactions into Modification Mode in case they have a "No Selection" Bucket</remarks>
    public async Task ProposeBuckets()
    {
        CurrentFilter = TransactionViewModelFilter.InModification;
        var unassignedTransactions = _transactions.Where(i => i.Buckets.First().SelectedBucket.BucketId == 0);
        ProposeBucketsCount = unassignedTransactions.Count();
        ProposeBucketsProgress = 0;

        var proposalTasks = unassignedTransactions.Select(transaction => 
            Task.Run(() =>
            {
                transaction.StartModification();
                transaction.ProposeBucket();
                ProposeBucketsProgress++;
                ProposeBucketsPercentage = ProposeBucketsCount == 0 ? 
                    0 : Convert.ToInt32(Decimal.Divide(ProposeBucketsProgress, ProposeBucketsCount) * 100);
                BucketProposalProgressChanged?.Invoke(this, 
                    new ProposeBucketChangedEventArgs(ProposeBucketsProgress, ProposeBucketsPercentage));
            }));

        await Task.WhenAll(proposalTasks);
    }
}
