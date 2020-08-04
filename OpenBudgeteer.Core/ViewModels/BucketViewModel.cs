using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace OpenBudgeteer.Core.ViewModels
{
    public class BucketViewModel : ViewModelBase
    {
        private decimal _income;
        public decimal Income
        {
            get => _income;
            set => Set(ref _income, value);
        }

        private decimal _expenses;
        public decimal Expenses
        {
            get => _expenses;
            set => Set(ref _expenses, value);
        }

        private decimal _monthBalance;
        public decimal MonthBalance
        {
            get => _monthBalance;
            set => Set(ref _monthBalance, value);
        }

        private decimal _budget;
        public decimal Budget
        {
            get => _budget;
            set => Set(ref _budget, value);
        }

        private decimal _bankBalance;
        public decimal BankBalance
        {
            get => _bankBalance;
            set => Set(ref _bankBalance, value);
        }

        private decimal _pendingWant;
        public decimal PendingWant
        {
            get => _pendingWant;
            set => Set(ref _pendingWant, value);
        }

        private decimal _remainingBudget;
        public decimal RemainingBudget
        {
            get => _remainingBudget;
            set => Set(ref _remainingBudget, value);
        }

        private decimal _negativeBucketBalance;
        public decimal NegativeBucketBalance
        {
            get => _negativeBucketBalance;
            set => Set(ref _negativeBucketBalance, value);
        }

        private ObservableCollection<BucketGroupViewModelItem> _bucketGroups;
        public ObservableCollection<BucketGroupViewModelItem> BucketGroups
        {
            get => _bucketGroups;
            set => Set(ref _bucketGroups, value);
        }

        private ObservableCollection<int> _months;
        public ObservableCollection<int> Months
        {
            get => _months;
            set => Set(ref _months, value);
        }

        public event ViewModelReloadRequiredHandler ViewModelReloadRequired;
        public delegate void ViewModelReloadRequiredHandler(ViewModelBase sender);

        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private readonly YearMonthSelectorViewModel _yearMonthViewModel;

        public BucketViewModel(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel)
        {
            _dbOptions = dbOptions;
            BucketGroups = new ObservableCollection<BucketGroupViewModelItem>();
            _yearMonthViewModel = yearMonthViewModel;
            //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
        }

        public async Task<Tuple<bool,string>> LoadDataAsync()
        {
            try
            {
                BucketGroups.Clear();
                using (var bucketGroupDbContext = new DatabaseContext(_dbOptions))
                {
                    var bucketGroups = bucketGroupDbContext.BucketGroup.OrderBy(i => i.Position);

                    foreach (var bucketGroup in bucketGroups)
                    {
                        var newBucketGroup = new BucketGroupViewModelItem(_dbOptions, bucketGroup, _yearMonthViewModel.CurrentMonth);
                        newBucketGroup.ViewModelReloadRequired += (sender) =>
                        {
                            ViewModelReloadRequired?.Invoke(this);
                        };
                        using (var bucketDbContext = new DatabaseContext(_dbOptions))
                        {
                            var buckets = bucketDbContext.Bucket
                                .Where(i => i.BucketGroupId == newBucketGroup.BucketGroup.BucketGroupId)
                                .OrderBy(i => i.Name);

                            var bucketItemTasks = new List<Task<BucketViewModelItem>>();

                            foreach (var bucket in buckets)
                            {
                                if (bucket.ValidFrom > _yearMonthViewModel.CurrentMonth) continue; // Bucket not yet active for selected month
                                if (bucket.IsInactive && bucket.IsInactiveFrom <= _yearMonthViewModel.CurrentMonth) continue; // Bucket no longer active for selected month
                                bucketItemTasks.Add(BucketViewModelItem.CreateAsync(_dbOptions, bucket, _yearMonthViewModel.CurrentMonth));
                            }

                            foreach (var bucket in await Task.WhenAll(bucketItemTasks))
                            {
                                bucket.ViewModelReloadRequired += (sender) =>
                                {
                                    ViewModelReloadRequired?.Invoke(this);
                                };
                                newBucketGroup.Buckets.Add(bucket);
                            }
                        }
                        BucketGroups.Add(newBucketGroup);
                    }
                }
                var (success, errorMessage) = UpdateBalanceFigures();
                if (!success) throw new Exception(errorMessage);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool,string> CreateGroup()
        {
            var newPosition = BucketGroups.Count == 0 ? 1 : BucketGroups.Last().BucketGroup.Position + 1;
            var newGroup = new BucketGroup
            {
                BucketGroupId = 0,
                Name = "New Bucket Group",
                Position = newPosition
            };
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                if (dbContext.CreateBucketGroup(newGroup) == 0) 
                    return new Tuple<bool, string>(false, "Unable to write changes to database"); 
            }
            BucketGroups.Add(new BucketGroupViewModelItem(_dbOptions, newGroup, _yearMonthViewModel.CurrentMonth)
            {
                InModification = true
            });
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool, string> DistributeBudget()
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var buckets = new List<BucketViewModelItem>();
                        foreach (var bucketGroup in BucketGroups)
                        {
                            buckets.AddRange(bucketGroup.Buckets);
                        }
                        foreach (var bucket in buckets)
                        {
                            if (bucket.Want == 0) continue;
                            bucket.InOut = bucket.Want;
                            var (success, errorMessage) = bucket.HandleInOutInput("Enter");
                            if (!success) throw new Exception(errorMessage);
                        }
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return new Tuple<bool, string>(false, $"Error during Budget distribution: {e.Message}");
                    }
                    
                }
            }
            //UpdateBalanceFigures(); // Should be done but not required because it will be done during ViewModel reload
            ViewModelReloadRequired?.Invoke(this);
            return new Tuple<bool, string>(true, string.Empty);
        }

        public Tuple<bool,string> UpdateBalanceFigures()
        {
            try
            {
                var buckets = new List<BucketViewModelItem>();
                foreach (var bucketGroup in BucketGroups)
                {
                    bucketGroup.TotalBalance = bucketGroup.Buckets.Sum(i => i.Balance);
                    buckets.AddRange(bucketGroup.Buckets);
                }

                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    // Get all Transactions which are not marked as "Transfer" for current YearMonth
                    var results = dbContext.BankTransaction
                        .Join(
                            dbContext.BudgetedTransaction,
                            bankTransaction => bankTransaction.TransactionId,
                            budgetedTransaction => budgetedTransaction.TransactionId,
                            (bankTransaction, budgetedTransaction) => new
                            {
                                TransactionId = bankTransaction.TransactionId,
                                TransactionDate = bankTransaction.TransactionDate,
                                Amount = budgetedTransaction.Amount,
                                BucketId = budgetedTransaction.BucketId
                            })
                        .Where(i =>
                            i.BucketId != 2 &&
                            i.TransactionDate.Year == _yearMonthViewModel.SelectedYear &&
                            i.TransactionDate.Month == _yearMonthViewModel.SelectedMonth)
                        .ToList();

                    Income = results
                        .Where(i => i.Amount > 0)
                        .Sum(i => i.Amount);

                    Expenses = results
                        .Where(i => i.Amount < 0)
                        .Sum(i => i.Amount);

                    MonthBalance = Income + Expenses;
                    BankBalance = dbContext.BankTransaction
                        .ToList()
                        .Where(i => i.TransactionDate < _yearMonthViewModel.CurrentMonth.AddMonths(1))
                        .Sum(i => i.Amount);

                    Budget = BankBalance - BucketGroups.Sum(i => i.TotalBalance);

                    PendingWant = buckets.Where(i => i.Want > 0).Sum(i => i.Want);
                    RemainingBudget = Budget - PendingWant;
                    NegativeBucketBalance = buckets.Where(i => i.Balance < 0).Sum(i => i.Balance);
                }
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Error during Balance recalculation: {e.Message}");
            }
            return new Tuple<bool, string>(true, string.Empty);
            
        }

        public Tuple<bool, string> SaveChanges(BucketViewModelItem bucket)
        {
            var result = bucket.SaveChanges();
            if (!result.Item1) return result;
            return UpdateBalanceFigures();
        }

        public Tuple<bool, string> CloseBucket(BucketViewModelItem bucket)
        {
            var result = bucket.CloseBucket();
            if (!result.Item1) return result;
            return UpdateBalanceFigures();
        }
    }
}
