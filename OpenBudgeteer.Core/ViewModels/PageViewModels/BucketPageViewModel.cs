using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class BucketPageViewModel : BucketListingViewModel
{
    private BucketGroup? _newBucketGroup;
    /// <summary>
    /// Helper property to handle creation of a new <see cref="BucketGroup"/>
    /// </summary>
    public BucketGroup? NewBucketGroup
    {
        get => _newBucketGroup;
        private set => Set(ref _newBucketGroup, value);
    }
    
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

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="yearMonthViewModel">ViewModel instance to handle selection of a year and month</param>
    public BucketPageViewModel(IServiceManager serviceManager, YearMonthSelectorViewModel yearMonthViewModel) 
        : base(serviceManager, yearMonthViewModel)
    {
    }

    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <param name="excludeInactive">Exclude Buckets which are marked as inactive</param>
    /// <param name="includeDefaults">Include system default Buckets like Transfer and Income</param>
    /// <returns>Object which contains information and results of this method</returns>
    public override async Task<ViewModelOperationResult> LoadDataAsync(bool excludeInactive = false, bool includeDefaults = false)
    {
        try
        {
            var baseResult = await base.LoadDataAsync(excludeInactive, includeDefaults);
            if (!baseResult.IsSuccessful) throw new Exception(baseResult.Message);
            var result = UpdateBalanceFigures();
            if (!result.IsSuccessful) throw new Exception(result.Message);
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
        }
    }

    /// <summary>
    /// Creates an initial <see cref="BucketGroup"/> and adds it to ViewModel
    /// Will be added on first position and updates all other <see cref="BucketGroup"/> Positions accordingly
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateEmptyGroup()
    {
        NewBucketGroup = new BucketGroup
        {
            Id = Guid.Empty,
            Name = "New Bucket Group",
            Position = 1
        };
        /*foreach (var bucketGroup in BucketGroups)
        {
            bucketGroup.BucketGroup.Position++;
            dbContext.UpdateBucketGroup(bucketGroup.BucketGroup);
        }
        if (dbContext.CreateBucketGroup(newBucketGroup) == 0) 
            return new ViewModelOperationResult(false, "Unable to write changes to database");

        var newBucketGroupViewModelItem =
            new BucketGroupViewModel(_dbOptions, newBucketGroup, _yearMonthViewModel.CurrentMonth)
            {
                InModification = true
            };
        BucketGroups.Insert(0, newBucketGroupViewModelItem);*/
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates a new <see cref="BucketGroup"/> and adds it to ViewModel and Database.
    /// Will be added on the requested position.
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateGroup()
    {
        try
        {
            if (NewBucketGroup is null) throw new Exception("Unable to create Bucket Group");
            if (NewBucketGroup.Name == string.Empty) throw new Exception( "Bucket Group Name cannot be empty");
        
            // Set Id to 0 to enable creation
            NewBucketGroup.Id = Guid.Empty;
            ServiceManager.BucketGroupService.Create(NewBucketGroup);
        
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Starts deletion process in the passed <see cref="BucketGroupViewModel"/> and updates positions of
    /// all other <see cref="BucketGroup"/> accordingly
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <param name="bucketGroup">Instance that needs to be deleted</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteGroup(BucketGroupViewModel bucketGroup)
    {
        try
        {
            ServiceManager.BucketGroupService.Delete(bucketGroup.BucketGroup);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Put money into all Buckets according to their Want. Saves the results to the database.
    /// </summary>
    /// <remarks>Doesn't consider any available Budget figures.</remarks>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DistributeBudget()
    {
        try
        {
            var buckets = new List<BucketViewModel>();
            foreach (var bucketGroup in BucketGroups)
            {
                buckets.AddRange(bucketGroup.Buckets);
            }
            foreach (var bucket in buckets.Where(i => i.Want > 0))
            {
                bucket.InOut = bucket.Want;
                //TODO Test if Database Transaction works here
                var result = bucket.HandleInOutInput();
                if (!result.IsSuccessful) throw new Exception(result.Message);
            }

            //UpdateBalanceFigures(); // Should be done but not required because it will be done during ViewModel reload
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
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
            var buckets = new List<BucketViewModel>();
            foreach (var bucketGroup in BucketGroups.Where(i => 
                         i.BucketGroup.Id != Guid.Parse("00000000-0000-0000-0000-000000000001")))
            {
                bucketGroup.TotalBalance = bucketGroup.Buckets.Sum(i => i.Balance);
                bucketGroup.TotalWant = bucketGroup.Buckets.Where(i => i.Want > 0).Sum(i => i.Want);
                bucketGroup.TotalIn = bucketGroup.Buckets.Sum(i => i.In);
                bucketGroup.TotalActivity = bucketGroup.Buckets.Sum(i => i.Activity);
                buckets.AddRange(bucketGroup.Buckets);
            }

            // Get all Transactions which are not marked as "Transfer" for current YearMonth
            var results = ServiceManager.BudgetedTransactionService
                .GetAllNonTransfer(
                    YearMonthViewModel.CurrentPeriod.Item1, 
                    YearMonthViewModel.CurrentPeriod.Item2)
                .ToList();
            
            Income = results
                .Where(i => i.Amount > 0)
                .Sum(i => i.Amount);

            Expenses = results
                .Where(i => i.Amount < 0)
                .Sum(i => i.Amount);

            MonthBalance = Income + Expenses;
            BankBalance = ServiceManager.BankTransactionService
                .GetAll(DateTime.MinValue, YearMonthViewModel.CurrentPeriod.Item2)
                .ToList()
                .Sum(i => i.Amount);


            Budget = BankBalance - BucketGroups.Sum(i => i.TotalBalance);

            PendingWant = BucketGroups.Sum(i => i.TotalWant);
            RemainingBudget = Budget - PendingWant;
            NegativeBucketBalance = buckets
                .Where(i => i.Balance < 0)
                .Sum(i => i.Balance);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during Balance recalculation: {e.Message}");
        }

        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Helper method to start Save process for the passed <see cref="BucketViewModel"/>
    /// </summary>
    /// <remarks>Triggers also update of ViewModel figures</remarks>
    /// <param name="bucket"><see cref="BucketViewModel"/> instance with modifications</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveChanges(BucketViewModel bucket)
    {
        var createUpdateResult = bucket.CreateOrUpdateBucket();
        if (!createUpdateResult.IsSuccessful) return createUpdateResult;
        var updateFiguresResult = UpdateBalanceFigures();
        return new ViewModelOperationResult(
            updateFiguresResult.IsSuccessful,
            createUpdateResult.ViewModelReloadRequired || updateFiguresResult.ViewModelReloadRequired);
    }

    /// <summary>
    /// Helper method to start Deletion process for the passed <see cref="BucketViewModel"/>
    /// </summary>
    /// <remarks>Triggers also update of ViewModel figures</remarks>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <param name="bucket"><see cref="BucketViewModel"/> instance with containing <see cref="Bucket"/> to be closed</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseBucket(BucketViewModel bucket)
    {
        var closeBucketResult = bucket.CloseBucket();
        if (!closeBucketResult.IsSuccessful) return closeBucketResult;
        var updateFiguresResult = UpdateBalanceFigures();
        return new ViewModelOperationResult(
            updateFiguresResult.IsSuccessful,
            closeBucketResult.ViewModelReloadRequired || updateFiguresResult.ViewModelReloadRequired);
    }
}