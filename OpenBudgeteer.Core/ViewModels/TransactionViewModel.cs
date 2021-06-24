using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.EventClasses;

namespace OpenBudgeteer.Core.ViewModels
{
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

        private ObservableCollection<TransactionViewModelItem> _transactions;
        /// <summary>
        /// Collection of loaded Transactions
        /// </summary>
        public ObservableCollection<TransactionViewModelItem> Transactions
        {
            get => _transactions;
            set => Set(ref _transactions, value);
        }

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
            Transactions = new ObservableCollection<TransactionViewModelItem>();
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
                Transactions.Clear();

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
                        Transactions.Add(transaction);
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
                Transactions.Clear();

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
                        Transactions.Add(transaction);
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
                Transactions.Clear();
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
                        Transactions.Add(transaction);
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
            NewTransaction = new TransactionViewModelItem(_dbOptions, _yearMonthViewModel);
            NewTransaction.Buckets.Add(new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel.CurrentMonth));
        }

        /// <summary>
        /// Helper method to start modification process for all Transactions
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
            using (var dbTransaction = new DatabaseContext(_dbOptions).Database.BeginTransaction())
            {
                try
                {
                    foreach (var transaction in Transactions)
                    {
                        var result = transaction.UpdateItem();
                        if (!result.IsSuccessful) throw new Exception(result.Message);
                    }
                    dbTransaction.Commit();
                    return new ViewModelOperationResult(true);
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    return new ViewModelOperationResult(false, e.Message);
                }
            }
        }

        /// <summary>
        /// Starts process to propose the right <see cref="Bucket"/> for all Transactions
        /// </summary>
        /// <remarks>Sets all Transactions into Modification Mode in case they have a "No Selection" Bucket</remarks>
        public void ProposeBuckets()
        {
            foreach (var transaction in Transactions)
            {
                // Check on "No Selection" Bucket
                if (transaction.Buckets.First().SelectedBucket.BucketId == 0)
                {
                    transaction.StartModification();
                    transaction.ProposeBucket();
                }
            }
        }
    }
}
