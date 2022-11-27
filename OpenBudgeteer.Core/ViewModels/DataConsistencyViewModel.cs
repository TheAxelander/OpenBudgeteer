using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using static OpenBudgeteer.Core.Models.DataConsistencyCheckResult;

namespace OpenBudgeteer.Core.ViewModels;

public class DataConsistencyViewModel : ViewModelBase
{
    private ObservableCollection<DataConsistencyCheckResult> _dataConsistencyChecks;
    /// <summary>
    /// Collection of all results from executed data consistency checks
    /// </summary>
    public ObservableCollection<DataConsistencyCheckResult> DataConsistencyChecks
    {
        get => _dataConsistencyChecks;
        private set => Set(ref _dataConsistencyChecks, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public DataConsistencyViewModel(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
        DataConsistencyChecks = new();
    }

    /// <summary>
    /// Executes all data consistency checks and stores the results in <see cref="DataConsistencyChecks"/>
    /// </summary>
    public async Task RunAllChecksAsync()
    {
        DataConsistencyChecks.Clear();
        var checkTasks = new List<Task<DataConsistencyCheckResult>>
        {
            CheckTransferReconciliationAsync(),
            CheckBucketBalanceAsync(),
            CheckBankTransactionWithoutBucketAssignmentAsync(),
            CheckBudgetedTransactionOutsideOfValidityDateAsync()
        };

        foreach (var result in await Task.WhenAll(checkTasks))
        {
            DataConsistencyChecks.Add(result);
        }
    }

    /// <summary>
    /// Checks reconciliation of all <see cref="BankTransaction"/> assigned to <see cref="Bucket"/> Transfer
    /// </summary>
    /// <returns>Result of Data Consistency Check</returns>
    public async Task<DataConsistencyCheckResult> CheckTransferReconciliationAsync()
    {
        return await Task.Run(() =>
        {
            var checkName = "Transfer Transaction reconciliation";
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var result = dbContext.BudgetedTransaction.Where(i => i.BucketId == 2).Sum(i => i.Amount);
                return result != 0 ?
                    new DataConsistencyCheckResult(checkName, StatusCode.Alert, 
                        $"Sum of all Transfer Transactions should be 0 but is {result}.", new List<string[]>()) :
                    new DataConsistencyCheckResult(checkName, StatusCode.Ok, 
                        "Sum of all Transfer Transactions is 0", new List<string[]>());
            }
        });
    }

    /// <summary>
    /// Checks if there is any <see cref="Bucket"/> which has a negative balance
    /// </summary>
    /// <returns>Result of Data Consistency Check</returns>
    public async Task<DataConsistencyCheckResult> CheckBucketBalanceAsync()
    {
        return await Task.Run(async () =>
        {
            var checkName = "Bucket balance";
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var results = new List<Tuple<StatusCode, string[]>>();
                var checkTasks = new List<Task>();

                foreach (var bucket in dbContext.Bucket.Where(i => i.BucketId > 2).ToList())
                {
                    checkTasks.Add(Task.Run(() =>
                    {
                        using (var checkDbContext = new DatabaseContext(_dbOptions))
                        {
                            var bucketBalance = checkDbContext.BudgetedTransaction
                                .Join(checkDbContext.BankTransaction,
                                    i => i.TransactionId,
                                    j => j.TransactionId,
                                    ((budgetedTransaction, bankTransaction) => new { budgetedTransaction, bankTransaction }))
                                .Where(i => i.budgetedTransaction.BucketId == bucket.BucketId)
                                .Select(i => i.budgetedTransaction)
                                .ToList()
                                .Sum(i => i.Amount);

                            bucketBalance += checkDbContext.BucketMovement
                                .Where(i => i.BucketId == bucket.BucketId)
                                .ToList()
                                .Sum(i => i.Amount);
                            results.Add(bucketBalance < 0 ?
                                new(StatusCode.Warning, new string[2] { bucket.Name, bucketBalance.ToString() }) :
                                new(StatusCode.Ok, new string[0]));
                        }
                    }));
                }

                await Task.WhenAll(checkTasks);

                if (results.Any(i => i.Item1 != StatusCode.Ok))
                {
                    var detailsBuilder = new List<string[]>()
                    {
                        new string[2] { "Bucket", "Amount" }
                    };

                    detailsBuilder.AddRange(results
                        .Where(i => i.Item1 != StatusCode.Ok)
                        .Select(i => i.Item2));
                    
                    return new DataConsistencyCheckResult(
                        checkName,
                        StatusCode.Warning,
                        "Negative Balances detected for Buckets",
                        detailsBuilder);
                }
                return new DataConsistencyCheckResult(
                    checkName, 
                    StatusCode.Ok, 
                    "No negative Balances detected for Buckets", 
                    new List<string[]>());
            }
        });
    }

    /// <summary>
    /// Checks if there is any <see cref="BankTransaction"/> which doesn't have a <see cref="Bucket"/> assigned
    /// </summary>
    /// <returns>Result of Data Consistency Check</returns>
    public async Task<DataConsistencyCheckResult> CheckBankTransactionWithoutBucketAssignmentAsync()
    {
        return await Task.Run(async () =>
        {
            var checkName = "Transactions without Bucket assignment";
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var results = new List<Tuple<StatusCode, string[]>>();
                var checkTasks = new List<Task>();

                foreach (var bankTransaction in dbContext.BankTransaction.ToList())
                {
                    checkTasks.Add(Task.Run(() =>
                    {
                        using (var checkDbContext = new DatabaseContext(_dbOptions))
                        {
                            if (!checkDbContext.BudgetedTransaction.Any(i => i.TransactionId == bankTransaction.TransactionId))
                            {
                                results.Add(new(
                                    StatusCode.Warning,
                                    new string[3]
                                    {
                                        bankTransaction.TransactionDate.ToShortDateString(),
                                        bankTransaction.Memo,
                                        bankTransaction.Amount.ToString()
                                    }));
                            }
                        }
                    }));
                }
                await Task.WhenAll(checkTasks);

                if (results.Any(i => i.Item1 != StatusCode.Ok))
                {
                    var detailsBuilder = new List<string[]>()
                    {
                        new string[3] { "Transaction Date", "Memo", "Amount" }
                    };

                    detailsBuilder.AddRange(results
                        .Where(i => i.Item1 != StatusCode.Ok)
                        .Select(i => i.Item2));

                    return new DataConsistencyCheckResult(
                        checkName,
                        StatusCode.Warning,
                        "Some Transactions do not have any Bucket assigned",
                        detailsBuilder);
                }
                return new DataConsistencyCheckResult(
                    checkName,
                    StatusCode.Ok,
                    "All Transactions are assigned to at least one Bucket",
                    new List<string[]>());
            }
        });
    }

    /// <summary>
    /// Checks if any <see cref="BankTransaction"/> has been assigned to a <see cref="Bucket"/> 
    /// outside of validity data range
    /// </summary>
    /// <returns>Result of Data Consistency Check</returns>
    public async Task<DataConsistencyCheckResult> CheckBudgetedTransactionOutsideOfValidityDateAsync()
    {
        return await Task.Run(async () =>
        {
            var checkName = "Budgeted Transaction outside of validity date";
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var results = new List<Tuple<StatusCode, string, List<BankTransaction>>>();
                var checkTasks = new List<Task>();

                foreach (var bucket in dbContext.Bucket.Where(i => i.IsInactive).ToList())
                {
                    checkTasks.Add(Task.Run(() =>
                    {
                        using (var checkDbContext = new DatabaseContext(_dbOptions))
                        {
                            var invalidTransactions = checkDbContext.BankTransaction
                               .Join(
                                   checkDbContext.BudgetedTransaction,
                                   i => i.TransactionId,
                                   j => j.TransactionId,
                                   (bankTransaction, budgetedTransaction) => new { bankTransaction, budgetedTransaction })
                               .Where(i =>
                                   i.budgetedTransaction.BucketId == bucket.BucketId &&
                                   i.bankTransaction.TransactionDate > bucket.IsInactiveFrom);

                            results.Add(invalidTransactions.Any() ?
                                new(StatusCode.Alert,
                                    bucket.Name,
                                    invalidTransactions.Select(i => i.bankTransaction).ToList()) :
                                new(StatusCode.Ok, bucket.Name, new List<BankTransaction>()));
                        }
                    }));
                }

                await Task.WhenAll(checkTasks);

                if (results.Any(i => i.Item1 != StatusCode.Ok))
                {
                    var detailsBuilder = new List<string[]>()
                    {
                        new string[4] { "Bucket", "Transaction Date", "Memo", "Amount" }
                    };

                    foreach (var result in results.Where(i => i.Item1 != StatusCode.Ok))
                    {
                        foreach (var transaction in result.Item3)
                        {
                            detailsBuilder.Add(new string[4] {
                                result.Item2,
                                transaction.TransactionDate.ToShortDateString(),
                                transaction.Memo,
                                transaction.Amount.ToString()
                                });
                        }
                    }

                    return new DataConsistencyCheckResult(
                        checkName,
                        StatusCode.Alert,
                        "Some Buckets have Transactions assigned after invalidity date",
                        detailsBuilder);
                }
                return new DataConsistencyCheckResult(
                    checkName,
                    StatusCode.Ok,
                    "No Buckets have Transactions assigned after invalidity date",
                    new List<string[]>());
            }
        });
    }
}