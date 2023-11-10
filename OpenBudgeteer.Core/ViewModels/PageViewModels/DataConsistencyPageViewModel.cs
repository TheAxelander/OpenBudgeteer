using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class DataConsistencyPageViewModel : ViewModelBase
{
    /// <summary>
    /// Collection of all results from executed data consistency checks
    /// </summary>
    public readonly ObservableCollection<DataConsistencyCheckResult> DataConsistencyChecks;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public DataConsistencyPageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
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
            CheckBudgetedTransactionOutsideOfValidityDateAsync(),
            CheckNegativeBankTransactionAssignedToIncomeAsync()
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
            const string checkName = "Transfer Transaction reconciliation";
            var result = ServiceManager.BudgetedTransactionService
                .GetAllTransfer()
                .Sum(i => i.Amount);
            return result != 0 
                ? new DataConsistencyCheckResult(checkName, DataConsistencyCheckResult.StatusCode.Alert,
                    $"Sum of all Transfer Transactions should be 0 but is {result}.", new List<string[]>()) 
                : new DataConsistencyCheckResult(checkName, DataConsistencyCheckResult.StatusCode.Ok, 
                    "Sum of all Transfer Transactions is 0", new List<string[]>());
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
            const string checkName = "Bucket balance";
            var results = new List<Tuple<DataConsistencyCheckResult.StatusCode, string[]>>();
            var checkTasks = ServiceManager.BucketService
                .GetAll()
                .Select(bucket => Task.Run(() =>
                {
                    var bucketBalance = ServiceManager.BudgetedTransactionService.GetAllFromBucket(bucket.Id)
                        .Sum(i => i.Amount);

                    bucketBalance += ServiceManager.BucketMovementService.GetAllFromBucket(bucket.Id)
                        .Sum(i => i.Amount);
                    results.Add(bucketBalance < 0
                        ? new(DataConsistencyCheckResult.StatusCode.Warning, new[]
                        {
                            bucket.Name, 
                            bucketBalance.ToString("C", CultureInfo.CurrentCulture)
                        })
                        : new(DataConsistencyCheckResult.StatusCode.Ok, Array.Empty<string>()));
                }))
                .ToList();

            await Task.WhenAll(checkTasks);

            if (results.All(i => i.Item1 == DataConsistencyCheckResult.StatusCode.Ok))
            {
                return new DataConsistencyCheckResult(
                    checkName, 
                    DataConsistencyCheckResult.StatusCode.Ok, 
                    "No negative Balances detected for Buckets", 
                    new List<string[]>());
            }

            var detailsBuilder = new List<string[]>()
            {
                new[] { "Bucket", "Amount" }
            };

            detailsBuilder.AddRange(results
                .Where(i => i.Item1 != DataConsistencyCheckResult.StatusCode.Ok)
                .Select(i => i.Item2));
                    
            return new DataConsistencyCheckResult(
                checkName,
                DataConsistencyCheckResult.StatusCode.Warning,
                "Negative Balances detected for Buckets",
                detailsBuilder);
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
            const string checkName = "Transactions without Bucket assignment";
            var results = new List<Tuple<DataConsistencyCheckResult.StatusCode, string[]>>();
            var checkTasks = ServiceManager.BankTransactionService
                .GetAll()
                .Select(bankTransaction => Task.Run(() =>
                {
                    if (!ServiceManager.BudgetedTransactionService.GetAllFromTransaction(bankTransaction.Id).Any())
                    {
                        results.Add(new(
                            DataConsistencyCheckResult.StatusCode.Warning, 
                            new[]
                            {
                                bankTransaction.TransactionDate.ToShortDateString(), 
                                bankTransaction.Memo ?? string.Empty, 
                                bankTransaction.Amount.ToString("C", CultureInfo.CurrentCulture)
                            }));
                    }
                }))
                .ToList();

            await Task.WhenAll(checkTasks);

            if (results.All(i => i.Item1 == DataConsistencyCheckResult.StatusCode.Ok))
            {
                return new DataConsistencyCheckResult(
                    checkName,
                    DataConsistencyCheckResult.StatusCode.Ok,
                    "All Transactions are assigned to at least one Bucket",
                    new List<string[]>());
            }

            var detailsBuilder = new List<string[]>()
            {
                new[] { "Transaction Date", "Memo", "Amount" }
            };

            detailsBuilder.AddRange(results
                .Where(i => i.Item1 != DataConsistencyCheckResult.StatusCode.Ok)
                .Select(i => i.Item2));

            return new DataConsistencyCheckResult(
                checkName,
                DataConsistencyCheckResult.StatusCode.Warning,
                "Some Transactions do not have any Bucket assigned",
                detailsBuilder);
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
            const string checkName = "Budgeted Transaction outside of validity date";
            var results = new List<Tuple<DataConsistencyCheckResult.StatusCode, string, List<BankTransaction>>>();
            var checkTasks = ServiceManager.BucketService
                .GetAll()
                .Where(i => i.IsInactive)
                .Select(bucket => Task.Run(() =>
                {
                    var invalidTransactions = ServiceManager.BudgetedTransactionService
                        .GetAllFromBucket(bucket.Id, bucket.IsInactiveFrom, DateTime.MaxValue)
                        .ToList();

                    results.Add(invalidTransactions.Any()
                        ? new(
                            DataConsistencyCheckResult.StatusCode.Alert, 
                            bucket.Name, 
                            invalidTransactions.Select(i => i.Transaction).ToList())
                        : new(
                            DataConsistencyCheckResult.StatusCode.Ok, 
                            bucket.Name, 
                            new List<BankTransaction>()));
                }))
                .ToList();

            await Task.WhenAll(checkTasks);

            if (results.All(i => i.Item1 == DataConsistencyCheckResult.StatusCode.Ok))
            {
                return new DataConsistencyCheckResult(
                    checkName,
                    DataConsistencyCheckResult.StatusCode.Ok,
                    "No Buckets have Transactions assigned after invalidity date",
                    new List<string[]>());
            }

            var detailsBuilder = new List<string[]>()
            {
                new[] { "Bucket", "Transaction Date", "Memo", "Amount" }
            };

            foreach (var result in results.Where(i => i.Item1 != DataConsistencyCheckResult.StatusCode.Ok))
            {
                detailsBuilder.AddRange(result.Item3
                    .Select(i => new[]
                    {
                        result.Item2, 
                        i.TransactionDate.ToShortDateString(), 
                        i.Memo ?? string.Empty, 
                        i.Amount.ToString("C", CultureInfo.CurrentCulture)
                    }));
            }

            return new DataConsistencyCheckResult(
                checkName,
                DataConsistencyCheckResult.StatusCode.Alert,
                "Some Buckets have Transactions assigned after invalidity date",
                detailsBuilder);
        });
    }

    /// <summary>
    /// Checks if any negative <see cref="BankTransaction"/> has been assigned to <see cref="Bucket"/> Income
    /// </summary>
    /// <returns>Result of Data Consistency Check</returns>
    public async Task<DataConsistencyCheckResult> CheckNegativeBankTransactionAssignedToIncomeAsync()
    {
        return await Task.Run(() =>
        {
            const string checkName = "Negative Bank Transaction assigned to Income";
            var bankTransactions = ServiceManager.BudgetedTransactionService
                .GetAllIncome()
                .Where(i => i.Amount < 0)
                .Select(i => i.Transaction)
                .ToList();

            var results = bankTransactions
                .Select(bankTransaction =>
                    new Tuple<DataConsistencyCheckResult.StatusCode, string, BankTransaction>(
                        DataConsistencyCheckResult.StatusCode.Warning,
                        checkName,
                        bankTransaction))
                .ToList();

            if (results.Count == 0)
            {
                return Task.FromResult(new DataConsistencyCheckResult(
                    checkName,
                    DataConsistencyCheckResult.StatusCode.Ok,
                    "All Transactions assigned to Income are positive",
                    new List<string[]>()));
            }

            var detailsBuilder = new List<string[]>()
            {
                new[] { "Transaction Date", "Memo", "Amount" }
            };

            detailsBuilder.AddRange(results
                .Select(i => new[]
                {
                    i.Item3.TransactionDate.ToShortDateString(),
                    i.Item3.Memo ?? string.Empty,
                    i.Item3.Amount.ToString("C", CultureInfo.CurrentCulture)
                }));

            return Task.FromResult(new DataConsistencyCheckResult(
                checkName,
                DataConsistencyCheckResult.StatusCode.Warning,
                "Some Transactions assigned to Income are negative",
                detailsBuilder));
        });
    }
}