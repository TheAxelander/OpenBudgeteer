using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

/// <summary>
/// This class must be inherited as displaying the data from the queries
/// depends on which visualization framework is used
/// </summary>
public abstract class ReportPageViewModel : ViewModelBase
{
    /// <summary>
    /// Helper class for Reports showing monthly Bucket expenses
    /// </summary>
    protected record MonthlyBucketExpensesReportResult
    {
        /// <summary>
        /// Name of the Bucket
        /// </summary>
        public readonly string BucketName;

        /// <summary>
        /// Collection of the results for the report
        /// </summary>
        public readonly List<Tuple<DateTime, decimal>> MonthlyResults;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="bucketName">Name of the <see cref="Bucket"/></param>
        /// <param name="monthlyResults">Query results with expenses per month</param>
        public MonthlyBucketExpensesReportResult(string bucketName, IEnumerable<Tuple<DateTime, decimal>> monthlyResults)
        {
            BucketName = bucketName;
            MonthlyResults = new List<Tuple<DateTime, decimal>>();
            foreach (var monthlyResult in monthlyResults)
            {
                MonthlyResults.Add(monthlyResult);
            }
        }
    }
    
    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    protected ReportPageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
    }
    
    /// <summary>
    /// Loads a set of balances per month from the database
    /// </summary>
    /// <remarks>Considers only <see cref="BankTransaction"/> within a month</remarks>
    /// <param name="months">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateTime"/> representing the month
    /// Item2: <see cref="decimal"/> representing the balance
    /// </returns>
    protected async Task<List<Tuple<DateTime, decimal>>> LoadMonthBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var transactions = ServiceManager.BankTransactionService
                .GetAll(currentMonth.AddMonths((months - 1) * -1), DateTime.MaxValue)
                .ToList();
            var monthBalances = transactions
                .GroupBy(i => new DateTime(i.TransactionDate.Year, i.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Balance = i.Sum(j => j.Amount)
                });

            return monthBalances
                .Select(group => new Tuple<DateTime, decimal>(group.YearMonth, group.Balance))
                .ToList();
        });
    }

    /// <summary>
    /// Loads a set of income and expenses per month from the database
    /// </summary>
    /// <param name="months">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateTime"/> representing the month
    /// Item2: <see cref="decimal"/> representing the income
    /// Item3: <see cref="decimal"/> representing the expenses
    /// </returns>
    protected async Task<List<Tuple<DateTime, decimal, decimal>>> LoadMonthIncomeExpensesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Get all Transactions which are not marked as "Transfer"
            var transactions = ServiceManager.BudgetedTransactionService
                .GetAllNonTransfer(currentMonth.AddMonths((months - 1) * -1), DateTime.MaxValue)
                .ToList();

            var monthIncomeExpenses = transactions
                .GroupBy(i => new DateTime(i.Transaction.TransactionDate.Year, i.Transaction.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                    Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                })
                .OrderBy(i => i.YearMonth);

            return monthIncomeExpenses
                .Select(group => new Tuple<DateTime, decimal, decimal>(group.YearMonth, group.Income, group.Expenses))
                .ToList();
        });
    }

    /// <summary>
    /// Loads a set of income and expenses per year from the database
    /// </summary>
    /// <param name="years">Number of years that should be loaded</param>
    /// <returns> 
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateTime"/> representing the year
    /// Item2: <see cref="decimal"/> representing the income
    /// Item3: <see cref="decimal"/> representing the expenses
    /// </returns>
    protected async Task<List<Tuple<DateTime, decimal, decimal>>> LoadYearIncomeExpensesAsync(int years = 5)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Get all Transactions which are not marked as "Transfer"
            var transactions = ServiceManager.BudgetedTransactionService
                    .GetAllNonTransfer(currentMonth.AddYears((years - 1) * -1), DateTime.MaxValue)
                    .ToList();

            var yearIncomeExpenses = transactions
                .GroupBy(i => new DateTime(i.Transaction.TransactionDate.Year, 1, 1))
                .Select(i => new
                {
                    Year = i.Key,
                    Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                    Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                })
                .OrderBy(i => i.Year);

            return yearIncomeExpenses
                .Select(group => new Tuple<DateTime, decimal, decimal>(group.Year, group.Income, group.Expenses))
                .ToList();
        });
    }

    /// <summary>
    /// Loads a set of balances per month from the database showing the progress of the overall bank balance
    /// </summary>
    /// <remarks>Considers all <see cref="BankTransaction"/> from the past</remarks>
    /// <param name="months">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateTime"/> representing the month
    /// Item2: <see cref="decimal"/> representing the balance
    /// </returns>
    protected async Task<List<Tuple<DateTime, decimal>>> LoadBankBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateTime, decimal>>();
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            for (int monthIndex = months - 1; monthIndex >= 0; monthIndex--)
            {
                var month = currentMonth.AddMonths(monthIndex * -1);
                //TODO Test if still works and consider rewrite for optimized query
                var lastDayOfMonth = month.AddMonths(1).AddDays(-1);
                var bankTransactions = ServiceManager.BankTransactionService
                    .GetAll(DateTime.MinValue, lastDayOfMonth)
                    .ToList();
                // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57)
                var bankBalance = bankTransactions.Sum(i => i.Amount);
                result.Add(new Tuple<DateTime, decimal>(month, bankBalance));
            }

            return result;
        });
    }

    /// <summary>
    /// Loads a set of expenses of a <see cref="Bucket"/> per month from the database
    /// </summary>
    /// <param name="month">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of ViewModelItems containing information about a <see cref="Bucket"/> and its expenses per month
    /// </returns>
    protected async Task<List<MonthlyBucketExpensesReportResult>> LoadMonthExpensesBucketAsync(int month = 12)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            var result = new List<MonthlyBucketExpensesReportResult>();

            foreach (var bucket in ServiceManager.BucketService
                         .GetActiveBuckets(DateTime.Now)
                         .Where(i => 
                             i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                             i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002")))
            {
                // Get latest BucketVersion based on passed parameter
                var latestVersion = ServiceManager.BucketService.GetLatestVersion(bucket.Id);
                if (latestVersion.BucketType != 2) continue;
                
                // Get Transactions for the current Bucket and the last x months
                var queryScope = ServiceManager.BudgetedTransactionService
                    .GetAllFromBucket(bucket.Id, currentMonth.AddMonths((month - 1) * -1), DateTime.MaxValue)
                    .ToList();
                // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57) 
                var queryResults = queryScope    
                    // Group the results per YearMonth
                    .GroupBy(i => new DateTime(i.Transaction.TransactionDate.Year, i.Transaction.TransactionDate.Month, 1))
                    // Create a new Grouped Object
                    .Select(i => new
                    {
                        YearMonth = i.Key,
                        Balance = (i.Sum(j => j.Transaction.Amount)) * -1
                    })
                    .OrderBy(i => i.YearMonth)
                    .ToList();

                // Collect results
                if (queryResults.Count == 0) continue; // No data available. Nothing to add
                var monthlyResults = new List<Tuple<DateTime, decimal>>();
                var reportInsertMonth = queryResults.First().YearMonth;
                foreach (var queryResult in queryResults)
                {
                    // Create empty MonthlyResults in case no data for specific months are available
                    while (queryResult.YearMonth != reportInsertMonth)
                    {
                        monthlyResults.Add(new Tuple<DateTime, decimal>(
                            reportInsertMonth,
                            0));
                        reportInsertMonth = reportInsertMonth.AddMonths(1);
                    }
                    monthlyResults.Add(new Tuple<DateTime, decimal>(
                        queryResult.YearMonth,
                        queryResult.Balance));
                    reportInsertMonth = reportInsertMonth.AddMonths(1);
                }
                result.Add(new MonthlyBucketExpensesReportResult(bucket.Name, monthlyResults));
            }

            return result;
        });
    }
}
