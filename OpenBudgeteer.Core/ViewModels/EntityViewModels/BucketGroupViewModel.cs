using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class BucketGroupViewModel : ViewModelBase
{
    private BucketGroup _bucketGroup;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BucketGroup BucketGroup
    {
        get => _bucketGroup;
        private set => Set(ref _bucketGroup, value);
    }

    private decimal _totalBalance;
    /// <summary>
    /// Balance of all Buckets assigned to the BucketGroup
    /// </summary>
    public decimal TotalBalance
    {
        get => _totalBalance;
        set => Set(ref _totalBalance, value);
    }

    private decimal _totalWant;
    /// <summary>
    /// Want of all Buckets assigned to the BucketGroup
    /// </summary>
    public decimal TotalWant
    {
        get => _totalWant;
        set => Set(ref _totalWant, value);
    }

    private decimal _totalIn;
    /// <summary>
    /// Sum of all <see cref="BucketMovement"/> from all Buckets assigned to the BucketGroup
    /// </summary>
    public decimal TotalIn
    {
        get => _totalIn;
        set => Set(ref _totalIn, value);
    }

    private decimal _totalActivity;
    /// <summary>
    /// Sum of money for all <see cref="BankTransaction"/> from all Buckets assigned to the BucketGroup
    /// </summary>
    public decimal TotalActivity
    {
        get => _totalActivity;
        set => Set(ref _totalActivity, value);
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

    private bool _isCollapsed;
    /// <summary>
    /// Helper property to check if the list of assigned Buckets is collapsed
    /// </summary>
    public bool IsCollapsed
    {
        get => _isCollapsed;
        set => Set(ref _isCollapsed, value);
    }

    private ObservableCollection<BucketViewModel> _buckets;
    /// <summary>
    /// Collection of Buckets assigned to this BucketGroup
    /// </summary>
    public ObservableCollection<BucketViewModel> Buckets
    {
        get => _buckets;
        private set => Set(ref _buckets, value);
    }

    private bool _inModification;
    /// <summary>
    /// Helper property to check if the BucketGroup is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        set => Set(ref _inModification, value);
    }
    
    private readonly DateTime _currentMonth;
    private BucketGroup? _oldBucketGroup;

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketGroup">BucketGroup instance</param>
    /// <param name="currentMonth">YearMonth that should be used</param>
    protected BucketGroupViewModel(IServiceManager serviceManager, BucketGroup bucketGroup, DateTime currentMonth) 
        : base(serviceManager)
    {
        _bucketGroup = bucketGroup;
        _buckets = new ObservableCollection<BucketViewModel>();
        _inModification = false;
        _currentMonth = currentMonth;
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketGroup">BucketGroup instance</param>
    /// <param name="currentMonth">YearMonth that should be used</param>
    public static BucketGroupViewModel CreateFromBucketGroup(IServiceManager serviceManager, BucketGroup bucketGroup, DateTime currentMonth)
    {
        return new BucketGroupViewModel(serviceManager, bucketGroup, currentMonth);
    }

    /// <summary>
    /// Helper method to start modification process and creating a backup of current values
    /// </summary>
    public void StartModification()
    {
        _oldBucketGroup = new BucketGroup()
        {
            Id = BucketGroup.Id,
            Name = BucketGroup.Name,
            Position = BucketGroup.Position
        };
        InModification = true;
    }

    /// <summary>
    /// Stops modification and restores previous values
    /// </summary>
    public void CancelModification()
    {
        BucketGroup = _oldBucketGroup ?? throw new Exception("Unexpected situation, no backup of Bucket Group available");
        InModification = false;
        _oldBucketGroup = null;
    }

    /// <summary>
    /// Updates a record in the database based on <see cref="BucketGroup"/> object
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveModification()
    {
        try
        {
            ServiceManager.BucketGroupService.Update(BucketGroup);
            InModification = false;
            _oldBucketGroup = null;
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to write changes to database: {e.Message}");
        }
    }

    /// <summary>
    /// Moves the position of the BucketGroup according to the passed value. Updates positions for all other
    /// BucketGroups accordingly
    /// </summary>
    /// <param name="positions">Number of positions that BucketGroup needs to be moved</param>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult MoveGroup(int positions)
    {
        try
        {
            ServiceManager.BucketGroupService.Move(BucketGroup.Id, positions);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to move Bucket Group: {e.Message}");
        }
    }

    public BucketViewModel CreateBucket()
    {
        var newBucket = BucketViewModel.CreateEmpty(ServiceManager, BucketGroup.Id, _currentMonth);
        Buckets.Add(newBucket);
        return newBucket;
    }
}