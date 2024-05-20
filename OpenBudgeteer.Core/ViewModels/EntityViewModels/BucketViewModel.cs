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

public class BucketViewModel : BaseEntityViewModel<Bucket>
{
    #region Properties & Fields
    
    /// <summary>
    /// Database Id of the Bucket
    /// </summary>
    public readonly Guid BucketId;
    
    private string _name;
    /// <summary>
    /// Name of the Bucket
    /// </summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }
    
    private BucketVersionViewModel _bucketVersion;
    /// <summary>
    /// Current Version of the Bucket
    /// </summary>
    public BucketVersionViewModel BucketVersion 
    { 
        get => _bucketVersion;
        set => Set(ref _bucketVersion, value);
    }
    
    private BucketGroup? _selectedBucketGroup;
    /// <summary>
    /// <see cref="BucketGroup"/> to which this Bucket is assigned to
    /// </summary>
    public BucketGroup? SelectedBucketGroup
    {
        get => _selectedBucketGroup; 
        set => Set(ref _selectedBucketGroup, value);
    }
    
    private string _colorCode;
    /// <summary>
    /// Name of the color based from <see cref="Color"/> for the Bucket Background
    /// </summary>
    public string ColorCode 
    { 
        get => _colorCode;
        set => Set(ref _colorCode, value);
    }
    
    /// <summary>
    /// Background <see cref="Color"/> of the Bucket
    /// </summary>
    public Color Color => string.IsNullOrEmpty(ColorCode) ? Color.LightGray : Color.FromName(ColorCode);
    
    private string _textColorCode;
    /// <summary>
    /// Name of the text color based from <see cref="Color"/>
    /// </summary>
    public string TextColorCode 
    { 
        get => _textColorCode;
        set => Set(ref _textColorCode, value);
    }
    
    /// <summary>
    /// Text <see cref="Color"/> of the Bucket
    /// </summary>
    public Color TextColor => string.IsNullOrEmpty(TextColorCode) ? Color.Black : Color.FromName(TextColorCode);
    
    private DateTime _validFrom;
    /// <summary>
    /// Date from which this Bucket is valid
    /// </summary>
    public DateTime ValidFrom 
    { 
        get => _validFrom;
        set => Set(ref _validFrom, value);
    }
    
    private bool _isInactive;
    /// <summary>
    /// Identifier if this Bucket is still active or not
    /// </summary>
    public bool IsInactive 
    { 
        get => _isInactive;
        set => Set(ref _isInactive, value);
    }
    
    private DateTime _isInactiveFrom;
    /// <summary>
    /// Date from which this Bucket started to be in status inactive
    /// </summary>
    public DateTime IsInactiveFrom 
    { 
        get => _isInactiveFrom;
        set => Set(ref _isInactiveFrom, value);
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
    /// Helper collection to list available System colors
    /// </summary>
    public readonly ObservableCollection<Color>? AvailableColors;

    /// <summary>
    /// Helper collection to list available <see cref="BucketGroup"/> where this Bucket can be assigned to
    /// </summary>
    public readonly ObservableCollection<BucketGroup>? AvailableBucketGroups;

    private readonly DateTime _currentYearMonth;
    
    #endregion
    
    #region Constructors

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
            BucketId = Guid.Empty;
            //BucketGroupId = bucketGroupId;    Will be set in CreateEmpty()
            _name = "New Bucket";
            _colorCode = Color.Transparent.Name;
            _textColorCode = Color.Black.Name;
            _validFrom = yearMonth;
            _isInactive = false;
            _isInactiveFrom = DateTime.MaxValue;

            _bucketVersion = BucketVersionViewModel.CreateEmpty(serviceManager);
            _bucketVersion.BucketTypeDateParameterChanged += CalculateBucketVersionNextApplyingDate;
        }
        else
        {
            BucketId = bucket.Id;
            _name = bucket.Name ?? string.Empty;
            _selectedBucketGroup = new BucketGroup() { Id = bucket.BucketGroupId };
            _colorCode = bucket.ColorCode ?? string.Empty;
            _textColorCode = bucket.TextColorCode ?? string.Empty;
            _validFrom = bucket.ValidFrom;
            _isInactive = bucket.IsInactive;
            _isInactiveFrom = bucket.IsInactiveFrom;
            
            _bucketVersion = BucketVersionViewModel.CreateFromBucket(serviceManager, bucket, _currentYearMonth);
            _bucketVersion.BucketTypeDateParameterChanged += CalculateBucketVersionNextApplyingDate;
            CalculateBucketVersionNextApplyingDate(this, EventArgs.Empty); // Run it once for initial calculation
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
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected BucketViewModel(BucketViewModel viewModel) : base(viewModel.ServiceManager)
    {
        BucketId = viewModel.BucketId;
        _name = viewModel.Name;
        _bucketVersion = (BucketVersionViewModel)viewModel.BucketVersion.Clone();
        _bucketVersion.BucketTypeDateParameterChanged += CalculateBucketVersionNextApplyingDate;
        _selectedBucketGroup = viewModel.SelectedBucketGroup;
        _colorCode = viewModel.ColorCode;
        _textColorCode = viewModel.TextColorCode;
        _validFrom = viewModel.ValidFrom;
        _isInactive = viewModel.IsInactive;
        _isInactiveFrom = viewModel.IsInactiveFrom;
        
        _balance = viewModel.Balance;
        _inOut = viewModel.InOut;
        _want = Want;
        _in = viewModel.In;
        _activity = viewModel.Activity;
        _details = viewModel.Details;
        _progress = viewModel.Progress;
        _isProgressBarVisible = viewModel.IsProgressbarVisible;
        _isHovered = viewModel.IsHovered;

        if (viewModel.AvailableColors != null)
        {
            AvailableColors = new ObservableCollection<Color>();
            foreach (var availableColor in viewModel.AvailableColors)
            {
                AvailableColors.Add(availableColor);
            }
        }

        if (viewModel.AvailableBucketGroups != null)
        {
            AvailableBucketGroups = new ObservableCollection<BucketGroup>();
            foreach (var item in viewModel.AvailableBucketGroups)
            {
                AvailableBucketGroups.Add(item);
            }
        }

        _currentYearMonth = viewModel._currentYearMonth;
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
            SelectedBucketGroup = availableBucketGroups.First(i => i.Id == bucketGroupId)
        };
    }
    
    /// <summary>
    /// Initialize ViewModel for displaying the <see cref="Bucket"/>. Not to be used for any modification purposes
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month, required for calculating various values</param>
    /// <returns>New ViewModel instance</returns>
    public static BucketViewModel CreateForListing(
        IServiceManager serviceManager,
        Bucket bucket, 
        DateTime yearMonth)
    {
        return new BucketViewModel(serviceManager, bucket, yearMonth);
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
        return await Task.Run(() => CreateForListing(serviceManager, bucket, yearMonth));
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new BucketViewModel(this);
    }

    #endregion
    
    #region Modification Handler
    
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
        
        /*#region Balance

        // Get all Transactions for this Bucket until passed yearMonth
        Balance = ServiceManager.BucketService.GetBalance(BucketId, _currentYearMonth);

        #endregion

        #region In & Activity

        var inOut = ServiceManager.BucketService.GetInAndOut(BucketId, _currentYearMonth);
        In = inOut.Item1;
        Activity = inOut.Item2;

        #endregion*/
        
        #region Balance, In & Out

        if (BucketId != default)
        {
            var figures = ServiceManager.BucketService.GetFigures(BucketId, _currentYearMonth);
            Balance = figures.Balance ?? 0;
            In = figures.Input;
            Activity = figures.Output;
        }
        
        #endregion

        #region Want

        if (!IsInactive)
        {
            switch (BucketVersion.BucketTypeParameter)
            {
                case BucketVersionViewModel.BucketType.StandardBucket:
                    break;
                case BucketVersionViewModel.BucketType.MonthlyExpense:
                    var newWant = BucketVersion.BucketTypeDecimalParameter - In;
                    Want = newWant < 0 ? 0 : newWant;
                    break;
                case BucketVersionViewModel.BucketType.ExpenseEveryXMonths:
                    var nextTargetDate = BucketVersion.BucketTypeDateParameter;
                    while (nextTargetDate < _currentYearMonth)
                    {
                        nextTargetDate = nextTargetDate.AddMonths(BucketVersion.BucketTypeIntParameter);
                    }
                    Want = CalculateWant(nextTargetDate);
                    break;
                case BucketVersionViewModel.BucketType.SaveXUntilYDate:
                    Want = CalculateWant(BucketVersion.BucketTypeDateParameter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        decimal CalculateWant(DateTime targetDate)
        {
            var remainingMonths = ((targetDate.Year - _currentYearMonth.Year) * 12) + targetDate.Month - _currentYearMonth.Month;
            if (remainingMonths < 0) return Balance < 0 ? Balance : 0;
            if (remainingMonths == 0 && Balance < 0) return Balance * -1;
            var wantForThisMonth = Math.Round((BucketVersion.BucketTypeDecimalParameter - Balance + In) / (remainingMonths + 1), 2) - In;
            if (remainingMonths == 0) wantForThisMonth += Activity; // check if target amount has been consumed. Not further Want required
            return wantForThisMonth < 0 ? 0 : wantForThisMonth;
        }

        #endregion

        #region Details

        if (BucketVersion.BucketTypeParameter is 
            BucketVersionViewModel.BucketType.ExpenseEveryXMonths or 
            BucketVersionViewModel.BucketType.SaveXUntilYDate)
        {
            var targetDate = BucketVersion.BucketTypeDateParameter;
            // Calculate new target date for BucketType 3 (Expense every X Months) 
            // if the selected yearMonth is already in the future
            if (BucketVersion.BucketTypeParameter == BucketVersionViewModel.BucketType.ExpenseEveryXMonths && 
                BucketVersion.BucketTypeDateParameter < _currentYearMonth)
            {
                do
                {
                    targetDate = targetDate.AddMonths(BucketVersion.BucketTypeIntParameter);
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
                Progress = Convert.ToInt32((Balance / BucketVersion.BucketTypeDecimalParameter) * 100);
            }
            
            // Some additional consistency checks and fixes
            if (Progress > 100) Progress = 100;
            if (Progress < 0) Progress = 0;
            
            Details = $"{BucketVersion.BucketTypeDecimalParameter} until {targetDate:yyyy-MM}";
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
    /// Converts the ViewModel back to the database object and returns it
    /// </summary>
    /// <returns>Converted ViewModel as Dto</returns>
    internal override Bucket ConvertToDto()
    {
        return new Bucket()
        {
            Id = BucketId,
            Name = Name,
            BucketGroupId = SelectedBucketGroup!.Id,
            ColorCode = ColorCode,
            TextColorCode = TextColorCode,
            ValidFrom = ValidFrom,
            IsInactive = IsInactive,
            IsInactiveFrom = IsInactiveFrom,
        };
    }

    /// <summary>
    /// Converts the ViewModel back to the database object and returns it
    /// </summary>
    /// <remarks>Includes a new <see cref="BucketVersion"/> for current month</remarks>
    /// <returns>Converted ViewModel as Dto</returns>
    private Bucket ConvertToDtoWithNewVersion()
    {
        var result = ConvertToDto();
        result.CurrentVersion = new BucketVersion()
        {
            Id = BucketVersion.BucketVersionId,
            Version = BucketVersion.Version, // API takes care for Version number increment
            BucketId = BucketId,
            ValidFrom = _currentYearMonth,
            BucketType = (int)BucketVersion.BucketTypeParameter,
            BucketTypeXParam = BucketVersion.BucketTypeIntParameter,
            BucketTypeYParam = BucketVersion.BucketTypeDecimalParameter,
            BucketTypeZParam = BucketVersion.BucketTypeDateParameter,
            Notes = BucketVersion.Notes
        };
        return result;
    }
    
    /// <summary>
    /// Creates or updates a record in the database based on ViewModel data
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
            if (BucketId == Guid.Empty)
            {
                ServiceManager.BucketService.Create(ConvertToDtoWithNewVersion());
            }
            else
            {
                ServiceManager.BucketService.Update(BucketVersion.HasModification
                    ? ConvertToDtoWithNewVersion()
                    : ConvertToDto());
            }
            CalculateValues();
            return new ViewModelOperationResult(true, true);
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
            if (BucketVersion.BucketTypeDecimalParameter < 0)
            {
                throw new Exception("Target amount must be positive");
            }

            // Check if target amount is 0 to prevent DivideByZeroException 
            if ((BucketVersion.BucketTypeParameter is 
                    BucketVersionViewModel.BucketType.MonthlyExpense or 
                    BucketVersionViewModel.BucketType.ExpenseEveryXMonths or 
                    BucketVersionViewModel.BucketType.SaveXUntilYDate) && 
                BucketVersion.BucketTypeDecimalParameter <= 0)
            {
                throw new Exception("Target amount must not be 0 for this Bucket Type.");
            }
            
            // Check if number of months is not 0
            if ((BucketVersion.BucketTypeParameter == BucketVersionViewModel.BucketType.ExpenseEveryXMonths) && 
                BucketVersion.BucketTypeIntParameter <= 0)
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
    /// Updates a record in the database based on ViewModel data to set it as inactive. In case there
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
            ServiceManager.BucketService.Close(BucketId, _currentYearMonth);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
        }
    }
    
    #endregion

    #region Misc

    /// <summary>
    /// Helper method to create a new <see cref="BucketMovement"/> record in the database based on User input
    /// </summary>
    /// <remarks>Recalculates figures after database operations</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult HandleInOutInput()
    {
        try
        {
            var date = DateTime.Now;
            if (_currentYearMonth.Year != date.Year || _currentYearMonth.Month != date.Month) {
                var day = (date > _currentYearMonth) ? DateTime.DaysInMonth(_currentYearMonth.Year, _currentYearMonth.Month) : 1;
                date = new DateTime(_currentYearMonth.Year, _currentYearMonth.Month, day);
            }
            ServiceManager.BucketService.CreateMovement(BucketId, InOut, date);
            CalculateValues();
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Error during database update: {e.Message}");
        }
    }

    /// <summary>
    /// Calculates the next applying target date based on current <see cref="BucketVersion"/> settings
    /// </summary>
    private void CalculateBucketVersionNextApplyingDate(object? sender, EventArgs e)
    {
        switch (BucketVersion.BucketTypeParameter)
        {
            case BucketVersionViewModel.BucketType.MonthlyExpense:
                BucketVersion.BucketTypeNextDateParameter = _currentYearMonth.AddMonths(1);
                break;
            case BucketVersionViewModel.BucketType.ExpenseEveryXMonths:
                var nextTargetDate = BucketVersion.BucketTypeDateParameter;
                while (nextTargetDate < _currentYearMonth)
                {
                    nextTargetDate = nextTargetDate.AddMonths(BucketVersion.BucketTypeIntParameter);
                }

                BucketVersion.BucketTypeNextDateParameter = nextTargetDate;
                break;
            case BucketVersionViewModel.BucketType.SaveXUntilYDate:
                BucketVersion.BucketTypeNextDateParameter = BucketVersion.BucketTypeDateParameter;
                break;
            case BucketVersionViewModel.BucketType.StandardBucket:
            default:
                BucketVersion.BucketTypeNextDateParameter = DateTime.MinValue;
                break;
        }       
    }
    
    #endregion
}