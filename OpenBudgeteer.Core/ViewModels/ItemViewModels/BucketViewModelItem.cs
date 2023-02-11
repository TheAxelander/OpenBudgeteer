using OpenBudgeteer.Core.Common.Database;
using OpenBudgeteer.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class BucketViewModelItem : ViewModelBase
{
    private Bucket _bucket;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public Bucket Bucket
    {
        get => _bucket;
        set => Set(ref _bucket, value);
    }

    private BucketVersion _bucketVersion;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BucketVersion BucketVersion
    {
        get => _bucketVersion;
        set => Set(ref _bucketVersion, value);
    }

    private decimal _balance;
    /// <summary>
    /// Overall Balance of a <see cref="Bucket"/> for the whole time
    /// </summary>
    public decimal Balance
    {
        get => _balance;
        set => Set(ref _balance, value);
    }

    private decimal _inOut;
    /// <summary>
    /// This will be just the input field for Bucket movements
    /// </summary>
    public decimal InOut
    {
        get => _inOut;
        set => Set(ref _inOut, value);
    }

    private decimal _want;
    /// <summary>
    /// Shows how many money a <see cref="Bucket"/> want to have for a specific month
    /// </summary>
    public decimal Want
    {
        get => _want;
        set => Set(ref _want, value);
    }

    private decimal _in;
    /// <summary>
    /// Sum of all <see cref="BucketMovement"/>
    /// </summary>
    public decimal In
    {
        get => _in;
        set => Set(ref _in, value);
    }

    private decimal _activity;
    /// <summary>
    /// Sum of money for all <see cref="BankTransaction"/> in a specific month
    /// </summary>
    public decimal Activity
    {
        get => _activity;
        set => Set(ref _activity, value);
    }

    private string _details;
    /// <summary>
    /// Contains information of the progress for <see cref="Bucket"/> with <see cref="BucketVersion.BucketType"/> 3 and 4
    /// </summary>
    public string Details
    {
        get => _details;
        set => Set(ref _details, value);
    }

    private int _progress;
    /// <summary>
    /// Contains the progress in %
    /// </summary>
    public int Progress
    {
        get => _progress;
        set => Set(ref _progress, value);
    }

    private bool _isProgressBarVisible;
    /// <summary>
    /// Helper property to set the visibility of the ProgressBar if <see cref="BucketVersion.BucketType"/> 3 or 4
    /// </summary>
    public bool IsProgressbarVisible
    {
        get => _isProgressBarVisible;
        set => Set(ref _isProgressBarVisible, value);
    }

    private bool _isHovered;
    /// <summary>
    /// Helper property to check if the cursor hovers over the entry in the UI
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set => Set(ref _isHovered, value);
    }

    private bool _inModification;
    /// <summary>
    /// Helper property to check if the Bucket is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        set => Set(ref _inModification, value);
    }

    private ObservableCollection<string> _availableBucketTypes;
    /// <summary>
    /// Helper collection to list BucketTypes explanations
    /// </summary>
    public ObservableCollection<string> AvailableBucketTypes
    {
        get => _availableBucketTypes;
        set => Set(ref _availableBucketTypes, value);
    }

    private ObservableCollection<Color> _availableColors;
    /// <summary>
    /// Helper collection to list available System colors
    /// </summary>
    public ObservableCollection<Color> AvailableColors
    {
        get => _availableColors;
        set => Set(ref _availableColors, value);
    }

    private ObservableCollection<BucketGroup> _availableBucketGroups;
    /// <summary>
    /// Helper collection to list available <see cref="BucketGroup"/> where this Bucket can be assigned to
    /// </summary>
    public ObservableCollection<BucketGroup> AvailableBucketGroups
    {
        get => _availableBucketGroups;
        set => Set(ref _availableBucketGroups, value);
    }

    private readonly bool _isNewlyCreatedBucket;
    private readonly DateTime _currentYearMonth;
    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public BucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
        AvailableBucketGroups = new ObservableCollection<BucketGroup>();
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            foreach (var item in dbContext.BucketGroup)
            {
                AvailableBucketGroups.Add(item);
            }
        }
        AvailableBucketTypes = new ObservableCollection<string>()
        {
            "Standard Bucket",
            "Monthly expense",
            "Expense every X Months",
            "Save X until Y date"
        };
        GetKnownColors();
        InModification = false;

        void GetKnownColors()
        {
            AvailableColors = new ObservableCollection<Color>();
            var colorType = typeof(Color);
            var propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (var propInfo in propInfos)
            {
                AvailableColors.Add(Color.FromName(propInfo.Name));
            }
        }
    }

    /// <summary>
    /// Initialize ViewModel based on a specific YearMonth
    /// </summary>
    /// <remarks>Creates an initial <see cref="BucketVersion"/></remarks>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="yearMonth">YearMonth that should be used</param>
    public BucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, DateTime yearMonth) : this(dbOptions)
    {
        _currentYearMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1);
        BucketVersion = new BucketVersion()
        {
            BucketId = Guid.Empty,
            BucketType = 1,
            BucketTypeZParam = yearMonth,
            ValidFrom = yearMonth
        };
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
    /// </summary>
    /// <remarks>Creates an initial <see cref="Bucket"/> in active modification mode</remarks>
    /// <remarks>Creates an initial <see cref="BucketVersion"/></remarks>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="bucketGroup">BucketGroup instance</param>
    /// <param name="yearMonth">YearMonth that should be used</param>
    public BucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, BucketGroup bucketGroup, DateTime yearMonth) : this(dbOptions, yearMonth)
    {
        _isNewlyCreatedBucket = true;
        InModification = true;
        Bucket = new Bucket()
        {
            BucketId = Guid.Empty,
            BucketGroupId = bucketGroup.BucketGroupId,
            Name = "New Bucket",
            ColorCode = Color.Transparent.Name,
            ValidFrom = yearMonth,
            IsInactive = false,
            IsInactiveFrom = DateTime.MaxValue
        };
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object and a specific YearMonth
    /// </summary>
    /// <remarks>Runs <see cref="CalculateValues"/> to get latest <see cref="BucketVersion"/></remarks>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">YearMonth that should be used</param>
    public BucketViewModelItem(DbContextOptions<DatabaseContext> dbOptions, Bucket bucket, DateTime yearMonth) : this(dbOptions, yearMonth)
    {
        Bucket = bucket;
        CalculateValues();
    }

    /// <summary>
    /// Creates and returns a new ViewModel based on an existing <see cref="Bucket"/> object and a specific YearMonth
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">YearMonth that should be used</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<BucketViewModelItem> CreateAsync(DbContextOptions<DatabaseContext> dbOptions, Bucket bucket, DateTime yearMonth)
    {
        return await Task.Run(() => new BucketViewModelItem(dbOptions, bucket, yearMonth));
    }

    /// <summary>
    /// Identifies latest <see cref="BucketVersion"/> based on <see cref="_currentYearMonth"/> and calculates all figures
    /// </summary>
    private void CalculateValues()
    {
        Balance = 0;
        In = 0;
        Activity = 0;
        Want = 0;
        InOut = 0;
        
        // Get latest BucketVersion based on passed parameter
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var bucketVersions = dbContext.BucketVersion
                .Where(i => i.BucketId == Bucket.BucketId)
                .OrderByDescending(i => i.ValidFrom)
                .ToList();
            //var orderedBucketVersions = bucketVersions.OrderByDescending(i => i.ValidFrom);
            foreach (var bucketVersion in bucketVersions)
            {
                if (bucketVersion.ValidFrom > _currentYearMonth) continue;
                BucketVersion = bucketVersion;
                break;
            }
            if (BucketVersion == null) throw new Exception("No Bucket Version found for the selected month");
        }
                    
        #region Balance

        // Get all Transactions for this Bucket until passed yearMonth
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            Balance += dbContext.BudgetedTransaction
                .Join(dbContext.BankTransaction,
                    i => i.TransactionId,
                    j => j.TransactionId,
                    ((budgetedTransaction, bankTransaction) => new { budgetedTransaction, bankTransaction }))
                .Where(i => i.budgetedTransaction.BucketId == Bucket.BucketId &&
                            i.bankTransaction.TransactionDate < _currentYearMonth.AddMonths(1))
                .Select(i => i.budgetedTransaction)
                .ToList()
                .Sum(i => i.Amount);

            Balance += dbContext.BucketMovement
                .Where(i => i.BucketId == Bucket.BucketId &&
                            i.MovementDate < _currentYearMonth.AddMonths(1))
                .ToList()
                .Sum(i => i.Amount);
        }

        #endregion

        #region In & Activity

        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            var bucketTransactionsCurrentMonth = dbContext.BudgetedTransaction
                .Join(dbContext.BankTransaction,
                    i => i.TransactionId,
                    j => j.TransactionId,
                    ((budgetedTransaction, bankTransaction) => new {budgetedTransaction, bankTransaction}))
                .Where(i => i.budgetedTransaction.BucketId == Bucket.BucketId &&
                            i.bankTransaction.TransactionDate.Year == _currentYearMonth.Year &&
                            i.bankTransaction.TransactionDate.Month == _currentYearMonth.Month)
                .Select(i => i.budgetedTransaction)
                .ToList();
            
            foreach (var bucketTransaction in bucketTransactionsCurrentMonth)
            {
                if (bucketTransaction.Amount < 0)
                    Activity += bucketTransaction.Amount;
                else
                    In += bucketTransaction.Amount;
            }

            var bucketMovementsCurrentMonth = dbContext.BucketMovement
                .Where(i => i.BucketId == Bucket.BucketId &&
                            i.MovementDate.Year == _currentYearMonth.Year &&
                            i.MovementDate.Month == _currentYearMonth.Month)
                .ToList();

            foreach (var bucketMovement in bucketMovementsCurrentMonth)
            {
                if (bucketMovement.Amount < 0)
                    Activity += bucketMovement.Amount;
                else
                    In += bucketMovement.Amount;
            }
        }

        #endregion

        #region Want

        if (!Bucket.IsInactive)
        {
            switch (BucketVersion.BucketType)
            {
                case 2:
                    var newWant = BucketVersion.BucketTypeYParam - In;
                    Want = newWant < 0 ? 0 : newWant;
                    break;
                case 3:
                    var nextTargetDate = BucketVersion.BucketTypeZParam;
                    while (nextTargetDate < _currentYearMonth)
                    {
                        nextTargetDate = nextTargetDate.AddMonths(BucketVersion.BucketTypeXParam);
                    }
                    Want = CalculateWant(nextTargetDate);
                    break;
                case 4:
                    Want = CalculateWant(BucketVersion.BucketTypeZParam);
                    break;
                default:
                    break;
            }
        }

        decimal CalculateWant(DateTime targetDate)
        {
            var remainingMonths = ((targetDate.Year - _currentYearMonth.Year) * 12) + targetDate.Month - _currentYearMonth.Month;
            if (remainingMonths < 0) return Balance < 0 ? Balance : 0;
            if (remainingMonths == 0 && Balance < 0) return Balance * -1;
            var wantForThisMonth = Math.Round((BucketVersion.BucketTypeYParam - Balance + In) / (remainingMonths + 1), 2) - In;
            if (remainingMonths == 0) wantForThisMonth += Activity; // check if target amount has been consumed. Not further Want required
            return wantForThisMonth < 0 ? 0 : wantForThisMonth;
        }

        #endregion

        #region Details

        if (BucketVersion.BucketType is 3 or 4)
        {
            var targetDate = BucketVersion.BucketTypeZParam;
            // Calculate new target date for BucketType 3 (Expense every X Months) 
            // if the selected yearMonth is already in the future
            if (BucketVersion.BucketType == 3 && BucketVersion.BucketTypeZParam < _currentYearMonth)
            {
                do
                {
                    targetDate = targetDate.AddMonths(BucketVersion.BucketTypeXParam);
                } while (targetDate < _currentYearMonth);
            }
            
            // Special Progress handling in target month with available activity, otherwise usual calculation
            if (_currentYearMonth.Month == targetDate.Month && 
                _currentYearMonth.Year == targetDate.Year &&
                Activity < 0)
            {
                Progress = Balance >= 0 ?
                    // Expense as expected or lower, hence target reached and Progress 100
                    100 :
                    // Expense in target month was higher than expected, hence negative Balance.
                    // Progress based on Want and Activity
                    Convert.ToInt32(100 - (Want / Activity * -1) * 100);

            }
            else
            {
                Progress = Convert.ToInt32((Balance / BucketVersion.BucketTypeYParam) * 100);
                if (Progress > 100) Progress = 100;
            }
            
            Details = $"{BucketVersion.BucketTypeYParam} until {targetDate:yyyy-MM}";
            IsProgressbarVisible = true;
        }
        else
        {
            Progress = 0;
            Details = string.Empty;
            IsProgressbarVisible = false;
        }

        #endregion
    }

    /// <summary>
    /// Activates modification mode
    /// </summary>
    public void EditBucket()
    {
        InModification = true;
    }

    /// <summary>
    /// Updates a record in the database based on <see cref="Bucket"/> object to set it as inactive. In case there
    /// are no <see cref="BankTransaction"/> nor <see cref="BucketMovement"/> assigned to it, it will be deleted
    /// completely from the database (including <see cref="BucketVersion"/>)
    /// In case of a full deletion all <see cref="BucketRuleSet"/> will be also deleted.
    /// </summary>
    /// <remarks>Bucket will be set to inactive for the next month</remarks>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseBucket()
    {
        if (Bucket.IsInactive) return new ViewModelOperationResult(false, "Bucket has been already set to inactive");
        if (Balance != 0) return new ViewModelOperationResult(false, "Balance must be 0 to close a Bucket");
        
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (dbContext.BudgetedTransaction.Any(i => i.BucketId == Bucket.BucketId) ||
                        dbContext.BucketMovement.Any(i => i.BucketId == Bucket.BucketId))
                    {
                        // Bucket will be set to inactive for the next month
                        Bucket.IsInactive = true;
                        Bucket.IsInactiveFrom = _currentYearMonth.AddMonths(1);
                        if (dbContext.UpdateBucket(Bucket) == 0) 
                            throw new Exception($"Unable to deactivate Bucket for next month.{Environment.NewLine}" +
                                                $"{Environment.NewLine}" +
                                                $"Bucket ID: {Bucket.BucketId}{Environment.NewLine}" +
                                                $"Bucket Target Inactive Date: {Bucket.IsInactiveFrom.ToShortDateString()}");
                    }
                    else
                    {
                        // Bucket has no transactions & movements, so it can be directly deleted from the database
                        // Delete Bucket
                        if (dbContext.DeleteBucket(Bucket) == 0) 
                            throw new Exception($"Unable to delete Bucket.{Environment.NewLine}" +
                                                $"{Environment.NewLine}" +
                                                $"Bucket ID: {Bucket.BucketId}{Environment.NewLine}");
                        
                        // Delete all BucketVersion which refer to this Bucket
                        var bucketVersions = dbContext.BucketVersion
                            .Where(i => i.BucketId == Bucket.BucketId)
                            .ToList();
                        foreach (var bucketVersion in bucketVersions)
                        {
                            if (dbContext.DeleteBucketVersion(bucketVersion) == 0) 
                                throw new Exception($"Unable to delete a Bucket Version.{Environment.NewLine}" +
                                                    $"{Environment.NewLine}" +
                                                    $"Bucket Version ID: {bucketVersion.BucketVersionId}{Environment.NewLine}" +
                                                    $"Bucket Version: {bucketVersion.Version}");
                        }
                        
                        // Delete all BucketRuleSet which refer to this Bucket
                        var bucketRuleSets = dbContext.BucketRuleSet
                            .Where(i => i.TargetBucketId == Bucket.BucketId)
                            .ToList();
                        foreach (var bucketRuleSet in bucketRuleSets)
                        {
                            if (dbContext.DeleteBucketRuleSet(bucketRuleSet) == 0)
                                throw new Exception($"Unable to delete a Bucket Rule.{Environment.NewLine}" +
                                                    $"{Environment.NewLine}" +
                                                    $"Bucket Rule ID: {bucketRuleSet.BucketRuleSetId}{Environment.NewLine}");
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
                }
            }
        }            
        return new ViewModelOperationResult(true, true);
    }

    /// <summary>
    /// Creates or updates a record in the database based on <see cref="Bucket"/> object
    /// </summary>
    /// <remarks>Creates also a new <see cref="BucketVersion"/> record in the database</remarks>
    /// <remarks>
    /// Recalculates figures after database operations in case <see cref="ViewModelReloadRequired"/> has not been triggered
    /// </remarks>
    /// <remarks>Can trigger <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateOrUpdateBucket()
    {
        var validationResult = ValidateData();
        if (!validationResult.IsSuccessful) return validationResult;
        var writeDataResult = _isNewlyCreatedBucket ? CreateBucket() : UpdateBucket();
        if (!writeDataResult.IsSuccessful || writeDataResult.ViewModelReloadRequired) return writeDataResult;
        InModification = false;
        CalculateValues();
        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Runs several validation rules to prevent unintended behavior 
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult ValidateData()
    {
        try
        {
            // Check if target amount is positive
            if (BucketVersion.BucketTypeYParam < 0)
            {
                throw new Exception("Target amount must be positive");
            }

            // Check if target amount is 0 to prevent DivideByZeroException 
            if ((BucketVersion.BucketType is 3 or 4) && BucketVersion.BucketTypeYParam == 0)
            {
                throw new Exception("Target amount must not be 0 for this Bucket Type.");
            }
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }

        return new ViewModelOperationResult(true);
    }

    /// <summary>
    /// Creates a new record in the database based on <see cref="Bucket"/> object
    /// </summary>
    /// <remarks>Creates also a new <see cref="BucketVersion"/> record in the database</remarks>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult CreateBucket()
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (dbContext.CreateBucket(Bucket) == 0)
                        throw new Exception("Unable to create new Bucket.");

                    var newBucketVersion = BucketVersion;
                    newBucketVersion.BucketId = Bucket.BucketId;
                    newBucketVersion.Version = 1;
                    newBucketVersion.ValidFrom = _currentYearMonth;
                    if (dbContext.CreateBucketVersion(newBucketVersion) == 0)
                        throw new Exception($"Unable to create new Bucket Version.{Environment.NewLine}" +
                                            $"{Environment.NewLine}" +
                                            $"Bucket ID: {newBucketVersion.BucketId}");

                    transaction.Commit();
                    return new ViewModelOperationResult(true, true);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new ViewModelOperationResult(
                        false, 
                        $"Error during database update: {e.Message}", 
                        true);
                }
            }
        }
    }

    /// <summary>
    /// Updates a record in the database based on <see cref="Bucket"/> object
    /// </summary>
    /// <remarks>Creates also a new <see cref="BucketVersion"/> record in the database</remarks>
    /// <remarks>Can trigger <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    private ViewModelOperationResult UpdateBucket()
    {
        using (var dbContext = new DatabaseContext(_dbOptions))
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    // Check on Bucket changes and update database
                    var dbBucket = dbContext.Bucket.First(i => i.BucketId == Bucket.BucketId);
                    if (dbBucket.Name != Bucket.Name ||
                        dbBucket.ColorCode != Bucket.ColorCode ||
                        dbBucket.BucketGroupId != Bucket.BucketGroupId)
                    {
                        // BucketGroup update requires special handling as ViewModel needs to trigger reload
                        // to force re-rendering of Blazor Page
                        //if (dbBucket.BucketGroupId != Bucket.BucketGroupId) forceViewModelReload = true;

                        if (dbContext.UpdateBucket(Bucket) == 0)
                            throw new Exception($"Error during database update: Unable to update Bucket.{Environment.NewLine}" +
                                                $"{Environment.NewLine}" +
                                                $"Bucket ID: {Bucket.BucketId}");
                    }

                    // Check on BucketVersion changes and create new BucketVersion
                    var dbBucketVersion =
                        dbContext.BucketVersion.First(i => i.BucketVersionId == BucketVersion.BucketVersionId);
                    if (dbBucketVersion.BucketType != BucketVersion.BucketType ||
                        dbBucketVersion.BucketTypeXParam != BucketVersion.BucketTypeXParam ||
                        dbBucketVersion.BucketTypeYParam != BucketVersion.BucketTypeYParam ||
                        dbBucketVersion.BucketTypeZParam != BucketVersion.BucketTypeZParam ||
                        dbBucketVersion.Notes != BucketVersion.Notes)
                    {
                        if (dbContext.BucketVersion.Any(i =>
                            i.BucketId == BucketVersion.BucketId && i.Version > BucketVersion.Version))
                            throw new Exception("Cannot create new Version as already a newer Version exists");

                        if (BucketVersion.ValidFrom == _currentYearMonth)
                        {
                            // Bucket Version modified in the same month,
                            // so just update the version instead of creating a new version
                            if (dbContext.UpdateBucketVersion(BucketVersion) == 0)
                                throw new Exception($"Unable to update Bucket Version.{Environment.NewLine}" +
                                                    $"{Environment.NewLine}" +
                                                    $"Bucket Version ID: {BucketVersion.BucketVersionId}" +
                                                    $"Bucket ID: {BucketVersion.BucketId}" +
                                                    $"Bucket Version: {BucketVersion.Version}" +
                                                    $"Bucket Version Start Date: {BucketVersion.ValidFrom.ToShortDateString()}");
                        }
                        else
                        {
                            BucketVersion.Version++;
                            BucketVersion.BucketVersionId = Guid.Empty;
                            BucketVersion.ValidFrom = _currentYearMonth;
                            if (dbContext.CreateBucketVersion(BucketVersion) == 0)
                                throw new Exception($"Unable to create new Bucket Version.{Environment.NewLine}" +
                                                    $"{Environment.NewLine}" +
                                                    $"Bucket ID: {BucketVersion.BucketId}" +
                                                    $"Bucket Version: {BucketVersion.Version}" +
                                                    $"Bucket Version Start Date: {BucketVersion.ValidFrom.ToShortDateString()}");
                        }
                    }
                    transaction.Commit();
                    return new ViewModelOperationResult(true, true);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new ViewModelOperationResult(
                        false,
                        $"Error during database update: {e.Message}",
                        true);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to create a new <see cref="BucketMovement"/> record in the database based on User input
    /// </summary>
    /// <remarks>Creation starts once Enter key is pressed</remarks>
    /// <remarks>Recalculates figures after database operations</remarks>
    /// <param name="key">Pressed key</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult HandleInOutInput(string key)
    {
        if (key != "Enter") return new ViewModelOperationResult(true);
        try
        {
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                var newMovement = new BucketMovement(Bucket, InOut, _currentYearMonth);
                if (dbContext.CreateBucketMovement(newMovement) == 0)
                    throw new Exception($"Unable to create new Bucket Movement.{Environment.NewLine}" +
                                        $"{Environment.NewLine}" +
                                        $"Bucket ID: {newMovement.BucketId}" +
                                        $"Amount: {newMovement.Amount}" +
                                        $"Movement Date: {newMovement.MovementDate.ToShortDateString()}");
            }
            //ViewModelReloadRequired?.Invoke(this);
            CalculateValues();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
        }
    }
}
