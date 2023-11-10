using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class BucketViewModel : ViewModelBase
{
    private Bucket _bucket;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public Bucket Bucket
    {
        get => _bucket;
        private set => Set(ref _bucket, value);
    }

    private BucketVersion? _bucketVersion;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BucketVersion? BucketVersion
    {
        get => _bucketVersion;
        private set => Set(ref _bucketVersion, value);
    }

    private decimal _balance;
    /// <summary>
    /// Overall Balance of a <see cref="Bucket"/> for the whole time
    /// </summary>
    public decimal Balance
    {
        get => _balance;
        private set => Set(ref _balance, value);
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
        private set => Set(ref _want, value);
    }

    private decimal _in;
    /// <summary>
    /// Sum of all <see cref="BucketMovement"/>
    /// </summary>
    public decimal In
    {
        get => _in;
        private set => Set(ref _in, value);
    }

    private decimal _activity;
    /// <summary>
    /// Sum of money for all <see cref="BankTransaction"/> in a specific month
    /// </summary>
    public decimal Activity
    {
        get => _activity;
        private set => Set(ref _activity, value);
    }

    private string _details;
    /// <summary>
    /// Contains information of the progress for <see cref="Bucket"/> with <see cref="BucketVersion.BucketType"/> 3 and 4
    /// </summary>
    public string Details
    {
        get => _details;
        private set => Set(ref _details, value);
    }

    private int _progress;
    /// <summary>
    /// Contains the progress in %
    /// </summary>
    public int Progress
    {
        get => _progress;
        private set => Set(ref _progress, value);
    }

    private bool _isProgressBarVisible;
    /// <summary>
    /// Helper property to set the visibility of the ProgressBar if <see cref="BucketVersion.BucketType"/> 3 or 4
    /// </summary>
    public bool IsProgressbarVisible
    {
        get => _isProgressBarVisible;
        private set => Set(ref _isProgressBarVisible, value);
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

    /// <summary>
    /// Helper collection to list BucketTypes explanations
    /// </summary>
    public readonly ObservableCollection<string>? AvailableBucketTypes;

    /// <summary>
    /// Helper collection to list available System colors
    /// </summary>
    public readonly ObservableCollection<Color>? AvailableColors;

    /// <summary>
    /// Helper collection to list available <see cref="BucketGroup"/> where this Bucket can be assigned to
    /// </summary>
    public readonly ObservableCollection<BucketGroup>? AvailableBucketGroups;

    private readonly DateTime _currentYearMonth;

    /// <summary>
    /// Minimalistic constructor, only to be used for displaying a <see cref="Bucket"/>
    /// </summary>
    /// <remarks>Runs <see cref="CalculateValues"/> to get latest <see cref="BucketVersion"/></remarks>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    protected BucketViewModel(IServiceManager serviceManager, Bucket? bucket, DateTime yearMonth) : base(serviceManager)
    {
        _details = string.Empty;
        _currentYearMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1);
        
        if (bucket == null)
        {
            _bucket = new Bucket()
            {
                Id = Guid.Empty,
                /*BucketGroupId = bucketGroupId,*/ // Will be set in CreateEmpty()
                Name = "New Bucket",
                ColorCode = Color.Transparent.Name,
                ValidFrom = yearMonth,
                IsInactive = false,
                IsInactiveFrom = DateTime.MaxValue
            };
            _bucketVersion = new BucketVersion()
            {
                Id = Guid.Empty,
                BucketType = 1,
                BucketTypeZParam = yearMonth,
                ValidFrom = yearMonth
            };
        }
        else
        {
            _bucket = bucket;
            _bucketVersion = ServiceManager.BucketService.GetLatestVersion(bucket.Id, _currentYearMonth);
            // Run calculations, excluding system default Buckets
            if (bucket.BucketGroupId != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            {
                CalculateValues();    
            }
        }
    }
    
    /// <summary>
    /// Full constructor, used for modifying a <see cref="Bucket"/>
    /// </summary>
    /// <remarks>Runs <see cref="CalculateValues"/> to get latest <see cref="BucketVersion"/></remarks>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBucketGroups">List of all available <see cref="BucketGroup"/> from database. (Use a cached list here)</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    protected BucketViewModel(
        IServiceManager serviceManager,
        IEnumerable<BucketGroup> availableBucketGroups, 
        Bucket? bucket, 
        DateTime yearMonth) 
        : this(serviceManager, bucket, yearMonth)
    {
        // Collect Available Bucket Groups
        AvailableBucketGroups = new ObservableCollection<BucketGroup>();
        foreach (var item in availableBucketGroups)
        {
            AvailableBucketGroups.Add(item);
        }
        
        // Default set of Bucket Types
        AvailableBucketTypes = new ObservableCollection<string>()
        {
            "Standard Bucket",
            "Monthly expense",
            "Expense every X Months",
            "Save X until Y date"
        };
        
        // Get known Colors
        AvailableColors = new ObservableCollection<Color>();
        var colorType = typeof(Color);
        var propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
        foreach (var propInfo in propInfos)
        {
            AvailableColors.Add(Color.FromName(propInfo.Name));
        }
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object and a specific YearMonth
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBucketGroups">List of all available <see cref="BucketGroup"/> from database. (Use a cached list here)</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<BucketViewModel> CreateForModificationAsync(
        IServiceManager serviceManager,
        IEnumerable<BucketGroup> availableBucketGroups, 
        Bucket bucket, 
        DateTime yearMonth)
    {
        return await Task.Run(() => new BucketViewModel(serviceManager, availableBucketGroups, bucket, yearMonth));
    }
    
    /// <summary>
    /// Initialize ViewModel for creating a new <see cref="Bucket"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketGroupId">Id of the <see cref="BucketGroup"/> to which new <see cref="Bucket"/> will be assigned to</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    /// <returns>New ViewModel instance</returns>
    public static BucketViewModel CreateEmpty(
        IServiceManager serviceManager,
        Guid bucketGroupId,
        DateTime yearMonth)
    {
        var availableBucketGroups = serviceManager.BucketGroupService.GetAll().ToList();
        return new BucketViewModel(serviceManager, availableBucketGroups, null, yearMonth)
        {
            Bucket =
            {
                BucketGroupId = bucketGroupId
            }
        };
    }

    /// <summary>
    /// Initialize ViewModel for displaying the <see cref="Bucket"/>. Not to be used for any modification purposes
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    /// <returns>New ViewModel instance</returns>
    public static async Task<BucketViewModel> CreateForListingAsync(
        IServiceManager serviceManager,
        Bucket bucket, 
        DateTime yearMonth)
    {
        return await Task.Run(() => new BucketViewModel(serviceManager, bucket, yearMonth));
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
        BucketVersion = ServiceManager.BucketService.GetLatestVersion(Bucket.Id, _currentYearMonth);

        #region Balance

        // Get all Transactions for this Bucket until passed yearMonth
        Balance = ServiceManager.BucketService.GetBalance(Bucket.Id, _currentYearMonth);

        #endregion

        #region In & Activity

        var inOut = ServiceManager.BucketService.GetInAndOut(Bucket.Id, _currentYearMonth);
        In = inOut.Item1;
        Activity = inOut.Item2;

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
            }
            
            // Some additional consistency checks and fixes
            if (Progress > 100) Progress = 100;
            if (Progress < 0) Progress = 0;
            
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
    }

    /// <summary>
    /// Updates a record in the database based on <see cref="Bucket"/> object to set it as inactive. In case there
    /// are no <see cref="BankTransaction"/> nor <see cref="BucketMovement"/> assigned to it, it will be deleted
    /// completely from the database (including <see cref="BucketVersion"/>)
    /// In case of a full deletion all <see cref="BucketRuleSet"/> will be also deleted.
    /// </summary>
    /// <remarks>Bucket will be set to inactive for the next month</remarks>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CloseBucket()
    {
        try
        {
            ServiceManager.BucketService.Close(Bucket, _currentYearMonth);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
        }
    }

    /// <summary>
    /// Creates or updates a record in the database based on <see cref="Bucket"/> object
    /// </summary>
    /// <remarks>Creates also a new <see cref="BucketVersion"/> record in the database</remarks>
    /// <remarks>
    /// Recalculates figures after database operations in case <see cref="ViewModelOperationResult.ViewModelReloadRequired"/> has not been triggered
    /// </remarks>
    /// <remarks>Can trigger <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateOrUpdateBucket()
    {
        var validationResult = ValidateData();
        if (!validationResult.IsSuccessful) return validationResult;
        try
        {
            if (Bucket.Id == Guid.Empty)
                ServiceManager.BucketService.Create(Bucket, BucketVersion!, _currentYearMonth);
            else
                ServiceManager.BucketService.Update(Bucket, BucketVersion!, _currentYearMonth);
            CalculateValues();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}", true);
        }
        
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
            if (BucketVersion!.BucketTypeYParam < 0)
            {
                throw new Exception("Target amount must be positive");
            }

            // Check if target amount is 0 to prevent DivideByZeroException 
            if ((BucketVersion.BucketType is 2 or 3 or 4) && BucketVersion.BucketTypeYParam <= 0)
            {
                throw new Exception("Target amount must not be 0 for this Bucket Type.");
            }
            
            // Check if number of months is not 0
            if ((BucketVersion.BucketType == 3) && BucketVersion.BucketTypeXParam <= 0)
            {
                throw new Exception("Number of months must be positive for this Bucket Type.");
            }
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Helper method to create a new <see cref="BucketMovement"/> record in the database based on User input
    /// </summary>
    /// <remarks>Recalculates figures after database operations</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult HandleInOutInput()
    {
        try
        {
            ServiceManager.BucketService.CreateMovement(Bucket.Id, InOut, _currentYearMonth);
            CalculateValues();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
        }
    }
}