using OpenBudgeteer.Core.Common;
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

namespace OpenBudgeteer.Core.ViewModels
{
    public class TransactionViewModel : ViewModelBase
    {
        private TransactionViewModelItem _newTransaction;
        public TransactionViewModelItem NewTransaction
        {
            get => _newTransaction;
            set => Set(ref _newTransaction, value);
        }

        private ObservableCollection<TransactionViewModelItem> _transactions;
        public ObservableCollection<TransactionViewModelItem> Transactions
        {
            get => _transactions;
            set => Set(ref _transactions, value);
        }

        public event ViewModelReloadRequiredHandler ViewModelReloadRequired;
        public delegate void ViewModelReloadRequiredHandler(ViewModelBase sender);

        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private readonly YearMonthSelectorViewModel _yearMonthViewModel;

        public TransactionViewModel(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel)
        {
            _dbOptions = dbOptions;
            _yearMonthViewModel = yearMonthViewModel;
            ResetNewTransaction();
            Transactions = new ObservableCollection<TransactionViewModelItem>();
            //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
        }

        public async Task<Tuple<bool, string>> LoadDataAsync()
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
                        transaction.ViewModelReloadRequired += sender => ViewModelReloadRequired?.Invoke(this);
                        Transactions.Add(transaction);
                    }
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public async Task<Tuple<bool, string>> LoadDataAsync(Bucket bucket, bool withMovements)
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
                        transaction.ViewModelReloadRequired += sender => ViewModelReloadRequired?.Invoke(this);
                        Transactions.Add(transaction);
                    }
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public async Task<Tuple<bool, string>> LoadDataAsync(Account account)
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
                        transaction.ViewModelReloadRequired += sender => ViewModelReloadRequired?.Invoke(this);
                        Transactions.Add(transaction);
                    }
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> CreateItem()
        {
            NewTransaction.Transaction.TransactionId = 0; // Triggers CREATE during CreateUpdateTransaction()
            var (result, message) = NewTransaction.CreateItem();
            if (!result)
            {
                return new Tuple<bool, string>(false, message);
            }
            ResetNewTransaction();
            ViewModelReloadRequired?.Invoke(this);
            
            return new Tuple<bool, string>(true, string.Empty);
        }

        public void ResetNewTransaction()
        {
            NewTransaction = new TransactionViewModelItem(_dbOptions, _yearMonthViewModel);
            NewTransaction.Buckets.Add(new PartialBucketViewModelItem(_dbOptions, _yearMonthViewModel));
        }

        public void EditAllTransaction()
        {
            foreach (var transaction in Transactions)
            {
                transaction.StartModification();
            }
        }

        public Tuple<bool, string> SaveAllTransaction()
        {
            using (var dbTransaction = new DatabaseContext(_dbOptions).Database.BeginTransaction())
            {
                try
                {
                    foreach (var transaction in Transactions)
                    {
                        (bool success, string message) = transaction.UpdateItem();
                        if (!success) throw new Exception(message);
                    }
                    dbTransaction.Commit();
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    return new Tuple<bool, string>(false, e.Message);
                }
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public void CancelAllTransaction()
        {
            ViewModelReloadRequired?.Invoke(this);
        }
    }
}
