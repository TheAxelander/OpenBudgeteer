using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class BucketGroupViewModel : BaseEntityViewModel<BucketGroup>, IEquatable<BucketGroupViewModel>, IComparable<BucketGroupViewModel>
{
    #region Properties & Fields
    
    /// <summary>
    /// Database Id of the BucketGroup
    /// </summary>
    public readonly Guid BucketGroupId;

    private string _name;
    /// <summary>
    /// Name of the BucketGroup
    /// </summary>
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private int _position;
    /// <summary>
    /// Order position from all existing BucketGroups
    /// </summary>
    public int Position
    {
        get => _position;
        set => Set(ref _position, value);
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

    /// <summary>
    /// Collection of Buckets assigned to this BucketGroup
    /// </summary>
    public readonly ObservableCollection<BucketViewModel> Buckets;

    private bool _inModification;
    /// <summary>
    /// Helper property to check if the BucketGroup is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        private set => Set(ref _inModification, value);
    }
    
    private readonly DateOnly _currentMonth;
    private BucketGroupViewModel? _oldBucketGroup;

    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketGroup">BucketGroup instance</param>
    /// <param name="currentMonth">YearMonth that should be used</param>
    protected BucketGroupViewModel(IServiceManager serviceManager, BucketGroup? bucketGroup, DateOnly currentMonth) 
        : base(serviceManager, serviceManager.CreateLogger(typeof(BucketGroupViewModel)))
    {
        Buckets = new ObservableCollection<BucketViewModel>();
        _inModification = false;
        _currentMonth = currentMonth;

        if (bucketGroup is null)
        {
            BucketGroupId = Guid.Empty;
            _name = "New Bucket Group";
            Position = 1;
        }
        else
        {
            BucketGroupId = bucketGroup.Id;
            _name = bucketGroup.Name ?? string.Empty;
            _position = bucketGroup.Position;
        }
    }
    
    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected BucketGroupViewModel(BucketGroupViewModel viewModel) : base(viewModel.ServiceManager, viewModel.Logger)
    {
        BucketGroupId = viewModel.BucketGroupId;
        _name = viewModel.Name;
        _position = viewModel.Position;
        _currentMonth = viewModel._currentMonth;
        Buckets = new ObservableCollection<BucketViewModel>();
        foreach (var bucket in viewModel.Buckets)
        {
            Buckets.Add(bucket);
        }
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketGroup"/> object and a specific YearMonth
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketGroup">BucketGroup instance</param>
    /// <param name="currentMonth">YearMonth that should be used</param>
    public static BucketGroupViewModel CreateFromBucketGroup(IServiceManager serviceManager, BucketGroup bucketGroup, DateOnly currentMonth)
    {
        return new BucketGroupViewModel(serviceManager, bucketGroup, currentMonth);
    }
    
    /// <summary>
    /// Initialize ViewModel for creating a new <see cref="BucketGroup"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public static BucketGroupViewModel CreateEmpty(IServiceManager serviceManager)
    {
        return new BucketGroupViewModel(serviceManager, null, DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new BucketGroupViewModel(this);
    }

    #endregion
    
    #region Modification Handler
    
    /// <summary>
    /// Start modification process and create a backup of current ViewModel data
    /// </summary>
    public void StartModification()
    {
        _oldBucketGroup = new BucketGroupViewModel(this);
        InModification = true;
    }

    /// <summary>
    /// Stops modification and restores previous ViewModel data
    /// </summary>
    public void CancelModification()
    {
        if (_oldBucketGroup is null) return;
        Name = _oldBucketGroup.Name;
        Position = _oldBucketGroup.Position;
        InModification = false;
        _oldBucketGroup = null;
    }
    
    /// <summary>
    /// Convert current ViewModel into a corresponding <see cref="IEntity"/> object
    /// </summary>
    /// <returns>Converted ViewModel</returns>
    internal override BucketGroup ConvertToDto()
    {
        return new BucketGroup()
        {
            Id = BucketGroupId,
            Name = Name,
            Position = Position
        };
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
            ServiceManager.BucketGroupService.Create(ConvertToDto());
            return new ViewModelOperationResult(true, true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }

    /// <summary>
    /// Updates a record in the database based on ViewModel data
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveModification()
    {
        try
        {
            ServiceManager.BucketGroupService.Update(ConvertToDto());
            InModification = false;
            _oldBucketGroup = null;
            return new ViewModelOperationResult(true, true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }

    /// <summary>
    /// Starts deletion process based on ViewModel data and updates positions of
    /// all other <see cref="BucketGroup"/> accordingly
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteGroup()
    {
        try
        {
            ServiceManager.BucketGroupService.Delete(BucketGroupId);
            return new ViewModelOperationResult(true, true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }
    
    #endregion

    #region Misc 

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
            ServiceManager.BucketGroupService.Move(BucketGroupId, positions);
            return new ViewModelOperationResult(true, true);
        }
        catch (ServiceException e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred.");
            return new ViewModelOperationResult(false, "An unexpected error occurred. Please check the logs for more details.");
        }
    }
    
    #endregion

    #region IEquatable & IComparable Implementation

    public bool Equals(BucketGroupViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            BucketGroupId.Equals(other.BucketGroupId) && 
            _name == other._name && 
            _position == other._position && 
            _totalBalance == other._totalBalance && 
            _totalWant == other._totalWant && 
            _totalIn == other._totalIn && 
            _totalActivity == other._totalActivity;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BucketGroupViewModel)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(BucketGroupId);
        hashCode.Add(_name);
        hashCode.Add(_position);
        hashCode.Add(_totalBalance);
        hashCode.Add(_totalWant);
        hashCode.Add(_totalIn);
        hashCode.Add(_totalActivity);
        return hashCode.ToHashCode();
    }
    
    public int CompareTo(BucketGroupViewModel? other)
    {
        return string.Compare(_name, other?.Name, StringComparison.Ordinal);
    }
    
    public override string ToString() => Name;
    
    #endregion
}