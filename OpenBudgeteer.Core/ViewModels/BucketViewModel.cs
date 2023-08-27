using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using OpenBudgeteer.Contracts.Models;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Core.ViewModels;

public class BucketViewModel : ViewModelBase
{
    private decimal _income;
    /// <summary>
    /// Money that has been added to a Bucket
    /// </summary>
    public decimal Income
    {
        get => _income;
        private set => Set(ref _income, value);
    }

    private decimal _expenses;
    /// <summary>
    /// Money that has been moved out of the Bucket
    /// </summary>
    public decimal Expenses
    {
        get => _expenses;
        private set => Set(ref _expenses, value);
    }

    private decimal _monthBalance;
    /// <summary>
    /// Combined Income and Expenses in a specific month
    /// </summary>
    public decimal MonthBalance
    {
        get => _monthBalance;
        private set => Set(ref _monthBalance, value);
    }

    private decimal _budget;
    /// <summary>
    /// Available Money in a specific month
    /// </summary>
    public decimal Budget
    {
        get => _budget;
        private set => Set(ref _budget, value);
    }

    private decimal _bankBalance;
    /// <summary>
    /// Money available on all bank accounts
    /// </summary>
    public decimal BankBalance
    {
        get => _bankBalance;
        private set => Set(ref _bankBalance, value);
    }

    private decimal _pendingWant;
    /// <summary>
    /// Money expected to be added to a Bucket in a specific month
    /// </summary>
    public decimal PendingWant
    {
        get => _pendingWant;
        private set => Set(ref _pendingWant, value);
    }

    private decimal _remainingBudget;
    /// <summary>
    /// Remaining Money in a specific month. Includes Want and negative Balances
    /// </summary>
    public decimal RemainingBudget
    {
        get => _remainingBudget;
        private set => Set(ref _remainingBudget, value);
    }

    private decimal _negativeBucketBalance;
    /// <summary>
    /// Sum of all Bucket Balances where the number is negative
    /// </summary>
    public decimal NegativeBucketBalance
    {
        get => _negativeBucketBalance;
        private set => Set(ref _negativeBucketBalance, value);
    }

    private ObservableCollection<BucketGroupViewModelItem> _bucketGroups;
    /// <summary>
    /// Collection of Groups which contains a set of Buckets
    /// </summary>
    public ObservableCollection<BucketGroupViewModelItem> BucketGroups
    {
        get => _bucketGroups;
        private set => Set(ref _bucketGroups, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private readonly YearMonthSelectorViewModel _yearMonthViewModel;

    private bool _defaultCollapseState; // Keep Collapse State e.g. after YearMonth change of ViewModel reload

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
    public BucketViewModel(DbContextOptions<DatabaseContext> dbOptions, YearMonthSelectorViewModel yearMonthViewModel)
    {
        _dbOptions = dbOptions;
        BucketGroups = new ObservableCollection<BucketGroupViewModelItem>();
        _yearMonthViewModel = yearMonthViewModel;
        //_yearMonthViewModel.SelectedYearMonthChanged += (sender) => { LoadData(); };
    }

    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <param name="excludeInactive">Exclude Buckets which are marked as inactive</param>
    /// <param name="includeDefaults">Include system default Buckets like Transfer and Income</param>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync(bool excludeInactive = false, bool includeDefaults = false)
    {
        try
        {
            BucketGroups.Clear();
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                List<BucketGroup> bucketGroups;
                if (includeDefaults)
                {
                    bucketGroups = dbContext.BucketGroup
                    .OrderBy(i => i.Position)
                    .ToList();
                }
                else
                {
                    bucketGroups = dbContext.BucketGroup
                    .Where(i => i.BucketGroupId != Guid.Parse("00000000-0000-0000-0000-000000000001"))
                    .OrderBy(i => i.Position)
                    .ToList();
                }

                foreach (var bucketGroup in bucketGroups)
                {
                    var newBucketGroup = new BucketGroupViewModelItem(_dbOptions, bucketGroup, _yearMonthViewModel.CurrentMonth);
                    newBucketGroup.IsCollapsed = _defaultCollapseState;
                    var buckets = dbContext.Bucket
                            .Where(i => i.BucketGroupId == newBucketGroup.BucketGroup.BucketGroupId)
                            .OrderBy(i => i.Name)
                            .ToList();

                    var bucketItemTasks = new List<Task<BucketViewModelItem>>();
                    
                    foreach (var bucket in buckets)
                    {
                        if (excludeInactive && bucket.IsInactive) continue; // Skip as inactive Buckets should be excluded
                        if (bucket.ValidFrom > _yearMonthViewModel.CurrentMonth) continue; // Bucket not yet active for selected month
                        if (bucket.IsInactive && bucket.IsInactiveFrom <= _yearMonthViewModel.CurrentMonth) continue; // Bucket no longer active for selected month
                        bucketItemTasks.Add(BucketViewModelItem.CreateAsync(_dbOptions, bucket, _yearMonthViewModel.CurrentMonth));
                    }

                    foreach (var bucket in await Task.WhenAll(bucketItemTasks))
                    {
                        newBucketGroup.Buckets.Add(bucket);
                    }
                    BucketGroups.Add(newBucketGroup);
                }
            }
            var result = UpdateBalanceFigures();
            if (!result.IsSuccessful) throw new Exception(result.Message);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates an initial <see cref="BucketGroup"/> and adds it to ViewModel and Database.
    /// Will be added on first position and updates all other <see cref="BucketGroup"/> Positions accordingly
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateGroup()
    {
        var newGroup = new BucketGroup
        {
            BucketGroupId = Guid.Empty,
            Name = "New Bucket Group",
            Position = 1
        };
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            foreach (var bucketGroup in BucketGroups)
            {
                bucketGroup.BucketGroup.Position++;
                dbContext.UpdateBucketGroup(bucketGroup.BucketGroup);
            }
            if (dbContext.CreateBucketGroup(newGroup) == 0) 
                return new ViewModelOperationResult(false, "Unable to write changes to database");
        }

        var newBucketGroupViewModelItem =
            new BucketGroupViewModelItem(_dbOptions, newGroup, _yearMonthViewModel.CurrentMonth)
            {
                InModification = true
            };
        BucketGroups.Insert(0, newBucketGroupViewModelItem);
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates a new <see cref="BucketGroup"/> and adds it to ViewModel and Database.
    /// Will be added on the requested position.
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <param name="newBucketGroup">Instance of <see cref="BucketGroup"/> which needs to be created in database</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateGroup(BucketGroup newBucketGroup)
    {
        if (newBucketGroup is null)
            return new ViewModelOperationResult(false, "Unable to create Bucket Group");
        if (newBucketGroup.Name == string.Empty)
            return new ViewModelOperationResult(false, "Bucket Group Name cannot be empty");
        
        // Set Id to 0 to enable creation
        newBucketGroup.BucketGroupId = Guid.Empty;

        // Save Position, append Bucket Group and later move it to requested Position 
        var requestedPosition = newBucketGroup.Position;
        newBucketGroup.Position = BucketGroups.Count + 1;

        using var dbContext = new DatabaseContext(_dbOptions);
        if (dbContext.CreateBucketGroup(newBucketGroup) == 0) 
            return new ViewModelOperationResult(false, "Unable to write changes to database");

        var newlyCreatedBucketGroup = dbContext.BucketGroup.OrderBy(i => i.BucketGroupId).Last();
        var newBucketGroupViewModelItem = new BucketGroupViewModelItem(_dbOptions, newlyCreatedBucketGroup,
            _yearMonthViewModel.CurrentMonth);
        newBucketGroupViewModelItem.MoveGroup(requestedPosition - newBucketGroupViewModelItem.BucketGroup.Position);
            
        return new ViewModelOperationResult(true, true);
    }

    /// <summary>
    /// Starts deletion process in the passed <see cref="BucketGroupViewModelItem"/> and updates positions of
    /// all other <see cref="BucketGroup"/> accordingly
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <param name="bucketGroup">Instance that needs to be deleted</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteGroup(BucketGroupViewModelItem bucketGroup)
    {
        var index = BucketGroups.IndexOf(bucketGroup) + 1;
        var bucketGroupsToMove = BucketGroups.ToList().GetRange(index, BucketGroups.Count - index);

        using var dbContext = new DatabaseContext(_dbOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            if (bucketGroup.Buckets.Count > 0) throw new Exception("Groups with Buckets cannot be deleted.");
            dbContext.DeleteBucketGroup(bucketGroup.BucketGroup);
                    
            var dbBucketGroups = new List<BucketGroup>();
            foreach (var bucketGroupViewModelItem in bucketGroupsToMove)
            {
                bucketGroupViewModelItem.BucketGroup.Position -= 1;
                dbBucketGroups.Add(bucketGroupViewModelItem.BucketGroup);
            }

            dbContext.UpdateBucketGroups(dbBucketGroups);
                    
            transaction.Commit();
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Put money into all Buckets according to their Want. Saves the results to the database.
    /// </summary>
    /// <remarks>Doesn't consider any available Budget figures.</remarks>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DistributeBudget()
    {
        using var dbContext = new DatabaseContext(_dbOptions);
        using var transaction = dbContext.Database.BeginTransaction();
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
                var result = bucket.HandleInOutInput(dbContext);
                if (!result.IsSuccessful) throw new Exception(result.Message);
            }

            transaction.Commit();
            //UpdateBalanceFigures(); // Should be done but not required because it will be done during ViewModel reload
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return new ViewModelOperationResult(false, $"Error during Budget distribution: {e.Message}");
        }
    }

    /// <summary>
    /// Re-calculates figures of the ViewModel like Budget and Balances
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult UpdateBalanceFigures()
    {
        try
        {
            var buckets = new List<BucketViewModelItem>();
            foreach (var bucketGroup in BucketGroups.Where(i => 
                         i.BucketGroup.BucketGroupId != Guid.Parse("00000000-0000-0000-0000-000000000001")))
            {
                bucketGroup.TotalBalance = bucketGroup.Buckets.Sum(i => i.Balance);
                bucketGroup.TotalWant = bucketGroup.Buckets.Where(i => i.Want > 0).Sum(i => i.Want);
                bucketGroup.TotalIn = bucketGroup.Buckets.Sum(i => i.In);
                bucketGroup.TotalActivity = bucketGroup.Buckets.Sum(i => i.Activity);
                buckets.AddRange(bucketGroup.Buckets);
            }

            using var dbContext = new DatabaseContext(_dbOptions);
            // Get all Transactions which are not marked as "Transfer" for current YearMonth
            var results = dbContext.BudgetedTransaction
                .Include(i => i.Transaction)
                .Where(i =>
                    i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002") &&
                    i.Transaction.TransactionDate.Year == _yearMonthViewModel.SelectedYear &&
                    i.Transaction.TransactionDate.Month == _yearMonthViewModel.SelectedMonth)
                .ToList();
            
            Income = results
                .Where(i => i.Amount > 0)
                .Sum(i => i.Amount);

            Expenses = results
                .Where(i => i.Amount < 0)
                .Sum(i => i.Amount);

            MonthBalance = Income + Expenses;
            BankBalance = dbContext.BankTransaction
                .Where(i => i.TransactionDate < _yearMonthViewModel.CurrentMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);


            Budget = BankBalance - BucketGroups.Sum(i => i.TotalBalance);

            PendingWant = BucketGroups.Sum(i => i.TotalWant);
            RemainingBudget = Budget - PendingWant;
            NegativeBucketBalance = buckets.Where(i => i.Balance < 0).Sum(i => i.Balance);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during Balance recalculation: {e.Message}");
        }

        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Helper method to set Collapse status for all <see cref="BucketGroup"/>
    /// </summary>
    /// <param name="collapse">New collapse status</param>
    public void ChangeBucketGroupCollapse(bool collapse = true)
    {
        _defaultCollapseState = collapse;
        foreach (var bucketGroup in BucketGroups)
        {
            bucketGroup.IsCollapsed = collapse;
        }
    }

    /// <summary>
    /// Helper method to start Save process for the passed <see cref="BucketViewModelItem"/>
    /// </summary>
    /// <remarks>Triggers also update of ViewModel figures</remarks>
    /// <param name="bucket"><see cref="BucketViewModelItem"/> instance with modifications</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveChanges(BucketViewModelItem bucket)
    {
        var createUpdateResult = bucket.CreateOrUpdateBucket();
        if (!createUpdateResult.IsSuccessful) return createUpdateResult;
        var updateFiguresResult = UpdateBalanceFigures();
        return new ViewModelOperationResult(
            updateFiguresResult.IsSuccessful,
            createUpdateResult.ViewModelReloadRequired || updateFiguresResult.ViewModelReloadRequired);
    }

    /// <summary>
    /// Helper method to start Deletion process for the passed <see cref="BucketViewModelItem"/>
    /// </summary>
    /// <remarks>Triggers also update of ViewModel figures</remarks>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <param name="bucket"><see cref="BucketViewModelItem"/> instance with containing <see cref="Bucket"/> to be closed</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseBucket(BucketViewModelItem bucket)
    {
        var closeBucketResult = bucket.CloseBucket();
        if (!closeBucketResult.IsSuccessful) return closeBucketResult;
        var updateFiguresResult = UpdateBalanceFigures();
        return new ViewModelOperationResult(
            updateFiguresResult.IsSuccessful,
            closeBucketResult.ViewModelReloadRequired || updateFiguresResult.ViewModelReloadRequired);
    }
}
