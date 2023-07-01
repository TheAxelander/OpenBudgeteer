using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Contracts.Models;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.ViewModels
{
    public class BucketDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// Helper class for collecting <see cref="BudgetedTransaction"/> and <see cref="BucketMovement"/> results
        /// </summary>
        private class BucketActivity : BaseObject
        {
            private Guid _bucketId;
            public Guid BucketId
            {
                get => _bucketId;
                set => Set(ref _bucketId, value);
            }

            private DateTime _transactionDate;
            public DateTime TransactionDate
            {
                get => _transactionDate;
                set => Set(ref _transactionDate, value);
            }

            private decimal _amount;
            public decimal Amount
            {
                get => _amount;
                set => Set(ref _amount, value);
            }

            public BucketActivity(BudgetedTransaction budgetedTransaction)
            {
                BucketId = budgetedTransaction.BucketId;
                TransactionDate = budgetedTransaction.Transaction.TransactionDate;
                Amount = budgetedTransaction.Amount;
            }

            public BucketActivity(BucketMovement bucketMovement)
            {
                BucketId = bucketMovement.BucketId;
                TransactionDate = bucketMovement.MovementDate;
                Amount = bucketMovement.Amount;
            }
        }


        private Bucket _bucket;
        /// <summary>
        /// Current <see cref="Contracts.Models.Bucket"/> that will be used
        /// </summary>
        public Bucket Bucket
        {
            get => _bucket;
            set => Set(ref _bucket, value);
        }

        private TransactionViewModel _bucketMovementsData;
        /// <summary>
        /// Reused <see cref="TransactionViewModel"/> to display <see cref="BucketMovement"/> data
        /// </summary>
        public TransactionViewModel BucketMovementsData
        {
            get => _bucketMovementsData;
            set => Set(ref _bucketMovementsData, value);
        }

        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private readonly YearMonthSelectorViewModel _yearMonthViewModel;

        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <param name="dbOptions">Options to connect to a database</param>
        /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
        /// <param name="bucket">Bucket that should be used</param>
        public BucketDetailsViewModel(
            DbContextOptions<DatabaseContext> dbOptions, 
            YearMonthSelectorViewModel yearMonthViewModel,
            Bucket bucket)
        {
            _dbOptions = dbOptions;
            _yearMonthViewModel = yearMonthViewModel;
            Bucket = bucket;
            BucketMovementsData = new TransactionViewModel(dbOptions, _yearMonthViewModel);
        }

        /// <summary>
        /// Loads all <see cref="BankTransaction"/> assigned to the current <see cref="Bucket"/>. 
        /// Optionally <see cref="BucketMovement"/> will be transformed to <see cref="BankTransaction"/>.
        /// </summary>
        /// <param name="withMovements">Include <see cref="BucketMovement"/> which will be transformed to <see cref="BankTransaction"/></param>
        /// <returns>Object which contains information and results of this method</returns>
        public async Task<ViewModelOperationResult> LoadBucketMovementsDataAsync(bool withMovements)
        {
            return await BucketMovementsData.LoadDataAsync(Bucket, withMovements);
        }

        /// <summary>
        /// Loads balances per month for the current <see cref="Bucket"/> from the database
        /// </summary>
        /// <param name="months">Number of months that should be loaded</param>
        /// <returns>
        /// Collection of <see cref="Tuple"/> containing
        /// Item1: <see cref="DateTime"/> representing the month
        /// Item2: <see cref="decimal"/> representing the balance
        /// </returns>
        public async Task<List<Tuple<DateTime, decimal>>> LoadBucketMonthBalancesAsync(int months = 24)
        {
            return await Task.Run(() =>
            {
                var result = new List<Tuple<DateTime, decimal>>();
                var startingMonth = _yearMonthViewModel.CurrentMonth.AddMonths((months - 1) * -1);

                var transactions = GetAllBucketActivities(startingMonth);

                // Group Amount values per month
                var monthBalances = transactions
                .GroupBy(i => new DateTime(i.TransactionDate.Year, i.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Balance = i.Sum(j => j.Amount)
                })
                .OrderBy(i => i.YearMonth)
                .ToList();

                // Collect results
                for (var month = startingMonth; month <= _yearMonthViewModel.CurrentMonth; month = month.AddMonths(1))
                {
                    if (monthBalances.Count != 0)
                    {
                        var group = monthBalances.First();
                        if (month.Year == group.YearMonth.Year &&
                            month.Month == group.YearMonth.Month)
                        {
                            // Pop group into results
                            result.Add(new Tuple<DateTime, decimal>(group.YearMonth, group.Balance));
                            monthBalances.RemoveAt(0);
                            continue;
                        }
                    }
                    // There is a month in between without data, add empty element
                    result.Add(new Tuple<DateTime, decimal>(month, default));
                }

                return result;
            });
        }

        /// <summary>
        /// Loads a set of input and output per month for the current <see cref="Bucket"/> from the database
        /// </summary>
        /// <param name="months">Number of months that should be loaded</param>
        /// <returns>
        /// Collection of <see cref="Tuple"/> containing
        /// Item1: <see cref="DateTime"/> representing the month
        /// Item2: <see cref="decimal"/> representing the income
        /// Item3: <see cref="decimal"/> representing the expenses
        /// </returns>
        public async Task<List<Tuple<DateTime, decimal, decimal>>> LoadBucketMonthInOutAsync(int months = 24)
        {
            return await Task.Run(() =>
            {
                var result = new List<Tuple<DateTime, decimal, decimal>>();
                var startingMonth = _yearMonthViewModel.CurrentMonth.AddMonths((months - 1) * -1);

                var transactions = GetAllBucketActivities(startingMonth);

                // Group Input and Output values per month
                var monthBalances = transactions
                .GroupBy(i => new DateTime(i.TransactionDate.Year, i.TransactionDate.Month, 1))
                .Select(i => new
                {
                    YearMonth = i.Key,
                    Input = i.Where(j => j.Amount > 0).Sum(j => j.Amount),
                    Output = (i.Where(j => j.Amount < 0).Sum(j => j.Amount)) * -1
                })
                .OrderBy(i => i.YearMonth)
                .ToList();

                // Collect results
                for (var month = startingMonth; month <= _yearMonthViewModel.CurrentMonth; month = month.AddMonths(1))
                {
                    if (monthBalances.Count != 0)
                    {
                        var group = monthBalances.First();
                        if (month.Year == group.YearMonth.Year &&
                            month.Month == group.YearMonth.Month)
                        {
                            // Pop group into results
                            result.Add(new Tuple<DateTime, decimal, decimal>(group.YearMonth, group.Input, group.Output));
                            monthBalances.RemoveAt(0);
                            continue;
                        }
                    }
                    // There is a month in between without data, add empty element
                    result.Add(new Tuple<DateTime, decimal, decimal>(month, default, default));
                }

                return result;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startingMonth"></param>
        /// <returns></returns>
        private List<BucketActivity> GetAllBucketActivities(DateTime startingMonth)
        {
            using var dbContext = new DatabaseContext(_dbOptions);
            // Get all BankTransaction assigned to this Bucket
            var transactions = dbContext.BudgetedTransaction
                .Include(i => i.Transaction)
                .Where(i => i.Transaction.TransactionDate >= startingMonth)
                .Select(i => new BucketActivity(i))
                .ToList();

            // Append Bucket Movements
            transactions.AddRange(dbContext.BucketMovement
                .Where(i =>
                    i.BucketId == Bucket.BucketId &&
                    i.MovementDate >= startingMonth)
                .Select(i => new BucketActivity(i))
                .ToList());

            return transactions;
        }

        /// <summary>
        /// Loads balances per month for the current <see cref="Bucket"/> from the database 
        /// showing the progress of the overall balance
        /// </summary>
        /// <remarks>Considers all <see cref="BankTransaction"/> and <see cref="BucketMovement"/> from the past</remarks>
        /// <param name="months">Number of months that should be loaded</param>
        /// <returns>
        /// Collection of <see cref="Tuple"/> containing
        /// Item1: <see cref="DateTime"/> representing the month
        /// Item2: <see cref="decimal"/> representing the balance
        /// </returns>
        public async Task<List<Tuple<DateTime, decimal>>> LoadBucketBalanceProgressionAsync(int months = 24)
        {
            return await Task.Run(() =>
            {
                var result = new List<Tuple<DateTime, decimal>>();
                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                using var dbContext = new DatabaseContext(_dbOptions);
                for (int monthIndex = months - 1; monthIndex >= 0; monthIndex--)
                {
                    var month = currentMonth.AddMonths(monthIndex * -1);
                    var transactions = dbContext.BudgetedTransaction
                        .Include(i => i.Transaction)
                        .Where(i =>
                            i.BucketId == Bucket.BucketId &&
                            i.Transaction.TransactionDate < month.AddMonths(1))
                        .ToList();
                        
                    var bucketMovements = dbContext.BucketMovement
                        .Where(i =>
                            i.BucketId == Bucket.BucketId &&
                            i.MovementDate < month.AddMonths(1))
                        .ToList();

                    // Query split required due to incompatibility of decimal Sum operation on sqlite (see issue 57)
                    var bucketBalance = 
                        transactions.Sum(i => i.Amount) +
                        bucketMovements.Sum(i => i.Amount);
                    result.Add(new Tuple<DateTime, decimal>(month, bucketBalance));
                }

                return result;
            });
        }
    }
}
