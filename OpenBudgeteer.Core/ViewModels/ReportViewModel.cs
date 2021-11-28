using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels;

public class ReportViewModel : ViewModelBase
{
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public ReportViewModel(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
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
    public async Task<List<Tuple<DateTime, decimal>>> LoadMonthBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateTime, decimal>>();
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var transactions = dbContext.BankTransaction
                    .Where(i => i.TransactionDate >= currentMonth.AddMonths(months * -1))
                    .OrderBy(i => i.TransactionDate)
                    .ToList();
                var monthBalances = transactions
                    .GroupBy(i => new DateTime(i.TransactionDate.Year, i.TransactionDate.Month, 1))
                    .Select(i => new
                    {
                        YearMonth = i.Key,
                        Balance = i.Sum(j => j.Amount)
                    });

                foreach (var group in monthBalances)
                {
                    result.Add(new Tuple<DateTime, decimal>(group.YearMonth, group.Balance));
                }
            }

            return result;
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
    public async Task<List<Tuple<DateTime, decimal, decimal>>> LoadMonthIncomeExpensesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateTime, decimal, decimal>>();
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                // Get all Transactions which are not marked as "Transfer"
                var transactions = dbContext.BankTransaction
                    .Join(
                        dbContext.BudgetedTransaction,
                        bankTransaction => bankTransaction.TransactionId,
                        budgetedTransaction => budgetedTransaction.TransactionId,
                        (bankTransaction, budgetedTransaction) => new
                        {
                            bankTransaction.TransactionId,
                            bankTransaction.TransactionDate,
                            budgetedTransaction.Amount,
                            budgetedTransaction.BucketId
                        })
                    .Where(i =>
                        i.BucketId != 2 && 
                        i.TransactionDate >= currentMonth.AddMonths(months * -1))
                    .ToList();

                var monthIncomeExpenses = transactions
                    .GroupBy(i => new DateTime(i.TransactionDate.Year, i.TransactionDate.Month, 1))
                    .Select(i => new
                    {
                        YearMonth = i.Key,
                        Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                        Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                    })
                    .OrderBy(i => i.YearMonth);

                foreach (var group in monthIncomeExpenses)
                {
                    result.Add(new Tuple<DateTime, decimal, decimal>(group.YearMonth, group.Income, group.Expenses));
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Loads a set of income and expenses per year from the database
    /// </summary>
    /// <param name="years">Number of years that should be loaded</param>
    /// Collection of <see cref="Tuple"/> containing
    /// Item1: <see cref="DateTime"/> representing the year
    /// Item2: <see cref="decimal"/> representing the income
    /// Item3: <see cref="decimal"/> representing the expenses
    /// </returns>
    public async Task<List<Tuple<DateTime, decimal, decimal>>> LoadYearIncomeExpensesAsync(int years = 5)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateTime, decimal, decimal>>();
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                // Get all Transactions which are not marked as "Transfer"
                var transactions = dbContext.BankTransaction
                    .Join(
                        dbContext.BudgetedTransaction,
                        bankTransaction => bankTransaction.TransactionId,
                        budgetedTransaction => budgetedTransaction.TransactionId,
                        (bankTransaction, budgetedTransaction) => new
                        {
                            bankTransaction.TransactionId,
                            bankTransaction.TransactionDate,
                            budgetedTransaction.Amount,
                            budgetedTransaction.BucketId
                        })
                    .Where(i =>
                        i.BucketId != 2 &&
                        i.TransactionDate >= currentMonth.AddYears(years * -1))
                    .ToList();

                var yearIncomeExpenses = transactions
                    .GroupBy(i => new DateTime(i.TransactionDate.Year, 1, 1))
                    .Select(i => new
                    {
                        Year = i.Key,
                        Income = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                        Expenses = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                    })
                    .OrderBy(i => i.Year);

                foreach (var group in yearIncomeExpenses)
                {
                    result.Add(new Tuple<DateTime, decimal, decimal>(group.Year, group.Income, group.Expenses));
                }
            }

            return result;
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
    public async Task<List<Tuple<DateTime, decimal>>> LoadBankBalancesAsync(int months = 24)
    {
        return await Task.Run(() =>
        {
            var result = new List<Tuple<DateTime, decimal>>();
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                for (int monthIndex = months; monthIndex >= 0; monthIndex--)
                {
                    var month = currentMonth.AddMonths(monthIndex * -1);
                    var bankTransactions = dbContext.BankTransaction
                        .Where(i => i.TransactionDate < month.AddMonths(1))
                        .OrderBy(i => i.TransactionDate)
                        .ToList();
                    // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57)
                    var bankBalance = bankTransactions.Sum(i => i.Amount);
                    result.Add(new Tuple<DateTime, decimal>(month, bankBalance));
                }
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
    public async Task<List<MonthlyBucketExpensesReportViewModelItem>> LoadMonthExpensesBucketAsync(int month = 12)
    {
        return await Task.Run(() =>
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
            var result = new List<MonthlyBucketExpensesReportViewModelItem>();

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var buckets = dbContext.Bucket
                    .Where(i => !i.IsInactive && i.BucketId > 2)
                    .OrderBy(i => i.Name);
                foreach (var bucket in buckets)
                {
                    // Get latest BucketVersion based on passed parameter
                    using (var bucketVersionDbContext = new DatabaseContext(_dbOptions))
                    {
                        var newReportRecord = new MonthlyBucketExpensesReportViewModelItem();
                        var latestVersion = bucketVersionDbContext.BucketVersion
                            .Where(i => i.BucketId == bucket.BucketId)
                            .ToList()
                            .OrderByDescending(i => i.Version)
                            .First();
                        if (latestVersion.BucketType != 2) continue;
                        using (var budgetedTransactionDbContext = new DatabaseContext(_dbOptions))
                        {
                            var queryScope = budgetedTransactionDbContext.BankTransaction
                                // Join with BudgetedTransaction
                                .Join(budgetedTransactionDbContext.BudgetedTransaction,
                                    transaction => transaction.TransactionId,
                                    budgetedTransaction => budgetedTransaction.TransactionId,
                                    ((transaction, budgetedTransaction) => new
                                    {
                                        Transaction = transaction,
                                        BudgetedTransaction = budgetedTransaction
                                    }))
                                // Limit on Transactions for the current Bucket and the last x months
                                .Where(i => i.BudgetedTransaction.BucketId == bucket.BucketId &&
                                        i.Transaction.TransactionDate >= currentMonth.AddMonths(month * -1))
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
                                .ToList();

                            // Collect results
                            if (queryResults.Count == 0) continue; // No data available. Nothing to add
                            newReportRecord.BucketName = bucket.Name;
                            var reportInsertMonth = queryResults.First().YearMonth;
                            foreach (var queryResult in queryResults)
                            {
                                // Create empty MonthlyResults in case no data for specific months are available
                                while (queryResult.YearMonth != reportInsertMonth)
                                {
                                    newReportRecord.MonthlyResults.Add(new Tuple<DateTime, decimal>(
                                        reportInsertMonth,
                                        0));
                                    reportInsertMonth = reportInsertMonth.AddMonths(1);
                                }
                                newReportRecord.MonthlyResults.Add(new Tuple<DateTime, decimal>(
                                        queryResult.YearMonth,
                                        queryResult.Balance));
                                reportInsertMonth = reportInsertMonth.AddMonths(1);
                            }
                        }
                        result.Add(newReportRecord);
                    }
                }
            }
            return result;
        });
    }
}
