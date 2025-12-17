using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

/// <summary>
/// This class must be inherited as displaying the data from the queries
/// depends on which visualization framework is used
/// </summary>
public abstract class ReportPageViewModel : ViewModelBase
{
    /// <summary>
    /// Helper record for Reports showing monthly Bucket expenses
    /// </summary>
    /// <param name="BucketName">Name of the Bucket</param>
    /// <param name="MonthlyResults">Collection of the results for the report</param>
    protected record MonthlyBucketExpensesReportResult(
        string BucketName,
        List<Tuple<DateOnly, decimal>> MonthlyResults);

    protected record BucketReportResult(
        List<Tuple<string, decimal>> BalancesPerBucketGroup,
        List<Tuple<string, decimal>> BalancesPerBucket,
        List<Tuple<string, decimal>> InPerBucketGroup,
        List<Tuple<string, decimal>> InPerBucket,
        List<Tuple<string, decimal>> ActivityPerBucketGroup,
        List<Tuple<string, decimal>> ActivityPerBucket,
        List<Tuple<string, List<Tuple<string, decimal>>>> RemainingBudgetPerBucket
        );
    
    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    protected ReportPageViewModel(IServiceManager serviceManager) 
        : base(serviceManager, serviceManager.CreateLogger(typeof(ReportPageViewModel)))
    {
    }
    
    /// <summary>
    /// Loads a set of balances per month from the database
    /// </summary>
    /// <remarks>Considers only <see cref="BankTransaction"/> within a month</remarks>
    /// <param name="months">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateOnly"/> representing the month
    /// Item2: <see cref="decimal"/> representing the balance
    /// </returns>
    protected async Task<List<Tuple<DateOnly, decimal>>> LoadMonthBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

            var transactions = ServiceManager.BudgetedTransactionService
                .GetAllForReporting(currentMonth.AddMonths((months - 1) * -1), DateOnly.MaxValue)
                .ToList();
            var monthBalances = transactions
                .GroupBy(i => new DateOnly(i.Transaction.TransactionDate.Year, i.Transaction.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Balance = i.Sum(j => j.Amount)
                })
                .OrderBy(i => i.YearMonth);

            return monthBalances
                .Select(group => new Tuple<DateOnly, decimal>(group.YearMonth, group.Balance))
                .ToList();
        });
    }

    /// <summary>
    /// Loads a set of income and expenses per month from the database
    /// </summary>
    /// <param name="months">Number of months that should be loaded</param>
    /// <returns>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateOnly"/> representing the month
    /// Item2: <see cref="decimal"/> representing the income
    /// Item3: <see cref="decimal"/> representing the expenses
    /// </returns>
    protected async Task<List<Tuple<DateOnly, decimal, decimal>>> LoadMonthIncomeExpensesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Get all Transactions which are not marked as "Transfer"
            var transactions = ServiceManager.BudgetedTransactionService
                .GetAllForReporting(currentMonth.AddMonths((months - 1) * -1), DateOnly.MaxValue)
                .ToList();

            var monthIncomeExpenses = transactions
                .GroupBy(i => new DateOnly(i.Transaction.TransactionDate.Year, i.Transaction.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                    Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                })
                .OrderBy(i => i.YearMonth);

            return monthIncomeExpenses
                .Select(group => new Tuple<DateOnly, decimal, decimal>(group.YearMonth, group.Income, group.Expenses))
                .ToList();
        });
    }

    /// <summary>
    /// Loads a set of income and expenses per year from the database
    /// </summary>
    /// <param name="years">Number of years that should be loaded</param>
    /// <returns> 
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateOnly"/> representing the year
    /// Item2: <see cref="decimal"/> representing the income
    /// Item3: <see cref="decimal"/> representing the expenses
    /// </returns>
    protected async Task<List<Tuple<DateOnly, decimal, decimal>>> LoadYearIncomeExpensesAsync(int years = 5)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Get all Transactions which are not marked as "Transfer"
            var transactions = ServiceManager.BudgetedTransactionService
                .GetAllForReporting(currentMonth.AddYears((years - 1) * -1), DateOnly.MaxValue)
                .ToList();

            var yearIncomeExpenses = transactions
                .GroupBy(i => new DateOnly(i.Transaction.TransactionDate.Year, 1, 1))
                .Select(i => new
                {
                    Year = i.Key,
                    Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                    Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                })
                .OrderBy(i => i.Year);

            return yearIncomeExpenses
                .Select(group => new Tuple<DateOnly, decimal, decimal>(group.Year, group.Income, group.Expenses))
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
    /// Item1: <see cref="DateOnly"/> representing the month
    /// Item2: <see cref="decimal"/> representing the balance
    /// </returns>
    protected async Task<List<Tuple<DateOnly, decimal>>> LoadBankBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateOnly, decimal>>();
            var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

            for (int monthIndex = months - 1; monthIndex >= 0; monthIndex--)
            {
                var month = currentMonth.AddMonths(monthIndex * -1);
                var lastDayOfMonth = month.AddMonths(1).AddDays(-1);
                var bankTransactions = ServiceManager.BankTransactionService
                    .GetAll(DateOnly.MinValue, lastDayOfMonth)
                    .ToList();
                // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57)
                var bankBalance = bankTransactions.Sum(i => i.Amount);
                result.Add(new Tuple<DateOnly, decimal>(month, bankBalance));
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
            var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);
            var result = new List<MonthlyBucketExpensesReportResult>();

            foreach (var bucket in ServiceManager.BucketService
                         .GetActiveBuckets(DateOnly.FromDateTime(DateTime.Today))
                         .Where(i => 
                             i.Id != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                             i.Id != Guid.Parse("00000000-0000-0000-0000-000000000002") &&
                             !i.IsHiddenFromSummaries))
            {
                // Check on right Bucket Type
                var latestVersion = bucket.BucketVersions!
                    .OrderByDescending(i => i.Version)
                    .First();
                if (latestVersion.BucketType != 2) continue;
                
                // Get Transactions for the current Bucket and the last x months
                var queryScope = ServiceManager.BudgetedTransactionService
                    .GetAllFromBucket(bucket.Id, currentMonth.AddMonths((month - 1) * -1), DateOnly.MaxValue)
                    .ToList();
                // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57) 
                var queryResults = queryScope    
                    // Group the results per YearMonth
                    .GroupBy(i => new DateOnly(i.Transaction.TransactionDate.Year, i.Transaction.TransactionDate.Month, 1))
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
                var monthlyResults = new List<Tuple<DateOnly, decimal>>();
                var reportInsertMonth = queryResults.First().YearMonth;
                foreach (var queryResult in queryResults)
                {
                    // Create empty MonthlyResults in case no data for specific months are available
                    while (queryResult.YearMonth != reportInsertMonth)
                    {
                        monthlyResults.Add(new Tuple<DateOnly, decimal>(
                            reportInsertMonth,
                            0));
                        reportInsertMonth = reportInsertMonth.AddMonths(1);
                    }
                    monthlyResults.Add(new Tuple<DateOnly, decimal>(
                        queryResult.YearMonth,
                        queryResult.Balance));
                    reportInsertMonth = reportInsertMonth.AddMonths(1);
                }
                result.Add(new MonthlyBucketExpensesReportResult(bucket.Name ?? string.Empty, monthlyResults));
            }

            return result;
        });
    }

    /// <summary>
    /// Collect and return a few statistics of Buckets using a <see cref="BucketListingViewModel"/>
    /// </summary>
    /// <param name="yearMonth">Optional, if figures should be calculated up until a specific month from the past</param>
    /// <returns>
    /// Collection of various Bucket statistics
    /// </returns>
    protected async Task<BucketReportResult> LoadBucketStatisticsAsync(YearMonthSelectorViewModel? yearMonth = null)
    {
        var listingViewModel = new BucketListingViewModel(ServiceManager, yearMonth);
        await listingViewModel.LoadDataForReportingAsync();
        
        var balancesPerBucketGroup = listingViewModel.BucketGroups
            .Where(i => i.TotalBalance > 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.TotalBalance))
            .ToList();
        
        var balancesPerBucket = listingViewModel.BucketGroups
            .SelectMany(i => i.Buckets)
            .Where(i => i.Balance > 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.Balance))
            .ToList();
        
        var inPerBucketGroup = listingViewModel.BucketGroups
            .Where(i => i.TotalIn > 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.TotalIn))
            .ToList();
        
        var inPerBucket = listingViewModel.BucketGroups
            .SelectMany(i => i.Buckets)
            .Where(i => i.In > 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.In))
            .ToList();
        
        var activityPerBucketGroup = listingViewModel.BucketGroups
            .Where(i => i.TotalActivity < 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.TotalActivity))
            .ToList();
        
        var activityPerBucket = listingViewModel.BucketGroups
            .SelectMany(i => i.Buckets)
            .Where(i => i.Activity < 0)
            .Select(i => new Tuple<string, decimal>(i.Name, i.Activity))
            .ToList();

        var remainingBudgetPerBucket = listingViewModel.BucketGroups
            .Select(i => new Tuple<string, List<Tuple<string, decimal>>>(
                i.Name,
                i.Buckets
                    .Where(bucket =>
                        bucket.BucketVersion.BucketTypeParameter is
                            BucketVersionViewModel.BucketType.StandardBucket or
                            BucketVersionViewModel.BucketType.MonthlyExpense) // Include only meaningful Bucket Types
                    .Where(bucket =>
                        bucket is not { Balance: 0, Activity: 0, In: 0 }) // Exclude Buckets which are "unused"
                    .Select(bucket => new Tuple<string, decimal>(
                        bucket.Name,
                        CalculateRemainingBudgetPercentage(bucket)))
                    .ToList()
                ))
            .Where(i => i.Item2.Count != 0) // Exclude BucketGroups which don't have anything meaningful
            .ToList();
        
        return new BucketReportResult(
            balancesPerBucketGroup,
            balancesPerBucket,
            inPerBucketGroup,
            inPerBucket,
            activityPerBucketGroup,
            activityPerBucket,
            remainingBudgetPerBucket);

        decimal CalculateRemainingBudgetPercentage(BucketViewModel bucket)
        {
            if (bucket.Balance == 0) return 0; // No money left, 0% remaining
            if (bucket.Balance + bucket.Activity * -1 == 0) return 0; // Cover edge case (e.g. Data defect), to prevent zero division (see #328)

            // Calculate remaining budget as percentage of starting balance (including carryovers)
            return bucket.Balance / (bucket.Balance + bucket.Activity * -1) * 100;
        }
    }
}
