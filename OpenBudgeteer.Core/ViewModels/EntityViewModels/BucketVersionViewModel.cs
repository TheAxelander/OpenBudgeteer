using System;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class BucketVersionViewModel : BaseEntityViewModel<BucketVersion>, IEquatable<BucketVersionViewModel>, IComparable<BucketVersionViewModel>
{
    #region Properties & Fields

    public enum BucketType
    {
        [StringValue("Standard Bucket")]
        StandardBucket = 1,
        [StringValue("Monthly expense")]
        MonthlyExpense = 2,
        [StringValue("Expense every X Months")]
        ExpenseEveryXMonths = 3,
        [StringValue("Save X until Y date")]
        SaveXUntilYDate = 4
    }
    
    private Guid _bucketVersionId;
    /// <summary>
    /// Database Id of the <see cref="BucketVersion"/> assigned to this Bucket
    /// </summary>
    public Guid BucketVersionId 
    { 
        get => _bucketVersionId;
        set => Set(ref _bucketVersionId, value);
    }
    
    private int _version;
    /// <summary>
    /// Version number of the <see cref="Bucket"/>
    /// </summary>
    public int Version 
    { 
        get => _version;
        set => Set(ref _version, value);
    }
    
    private Guid _bucketId;
    /// <summary>
    /// Database Id of the <see cref="Bucket"/> which this BucketVersion is assigned to
    /// </summary>
    public Guid BucketId 
    { 
        get => _bucketId;
        set => Set(ref _bucketId, value);
    }
    
    private DateOnly _validFrom;
    /// <summary>
    /// Date from which this BucketVersion applies
    /// </summary>
    public DateOnly ValidFrom 
    { 
        get => _validFrom;
        set => Set(ref _validFrom, value);
    }
    
    private BucketType _bucketTypeParameter;
    /// <summary>
    /// Type of the Bucket
    /// </summary>
    public BucketType BucketTypeParameter
    {
        get => _bucketTypeParameter;
        set
        {
            if (Set(ref _bucketTypeParameter, value)) HasModification = true;
        }
    }

    private int _bucketTypeIntParameter;
    /// <summary>
    /// Integer based parameter of the Bucket type
    /// </summary>
    public int BucketTypeIntParameter
    {
        get => _bucketTypeIntParameter;
        set
        {
            if (Set(ref _bucketTypeIntParameter, value)) HasModification = true;
        }
    }

    private decimal _bucketTypeDecimalParameter;
    /// <summary>
    /// Decimal based parameter of the Bucket type
    /// </summary>
    public decimal BucketTypeDecimalParameter
    {
        get => _bucketTypeDecimalParameter;
        set
        {
            if (Set(ref _bucketTypeDecimalParameter, value)) HasModification = true;
        }
    }

    private DateOnly _bucketTypeDateParameter;
    /// <summary>
    /// Date based parameter of the Bucket type
    /// </summary>
    public DateOnly BucketTypeDateParameter
    {
        get => _bucketTypeDateParameter;
        set
        {
            if (Set(ref _bucketTypeDateParameter, value))
            {
                HasModification = true;
                BucketTypeDateParameterChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    private DateOnly _bucketTypeNextDateParameter;
    /// <summary>
    /// Date based parameter of the Bucket type (calculating to the next applying date)
    /// </summary>
    public DateOnly BucketTypeNextDateParameter
    {
        get => _bucketTypeNextDateParameter; 
        set => Set(ref _bucketTypeNextDateParameter, value);
    }

    private string _notes;
    /// <summary>
    /// Notes of the Bucket
    /// </summary>
    public string Notes
    {
        get => _notes;
        set
        {
            if (Set(ref _notes, value)) HasModification = true;
        }
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
    
    public bool HasModification { get; private set; }
    
    /// <summary>
    /// EventHandler which will be invoked once <see cref="BucketTypeDateParameter"/> has been changed.
    /// Can be used to trigger the calculation of <see cref="BucketTypeNextDateParameter"/> 
    /// </summary>
    public event EventHandler<EventArgs>? BucketTypeDateParameterChanged; 
    
    #endregion

    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketVersion"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketVersion">BucketVersion instance</param>
    protected BucketVersionViewModel(IServiceManager serviceManager, BucketVersion? bucketVersion) : base(serviceManager)
    {
        if (bucketVersion is null)
        {
            BucketVersionId = Guid.Empty;
            _version = 0;
            //BucketId = bucketVersion.BucketId;    Will be set in Database creation phase
            //ValidFrom = bucketVersion.ValidFrom;  Will be set in Database creation phase
            _bucketTypeParameter = BucketType.StandardBucket;
            _bucketTypeIntParameter = 0;
            _bucketTypeDecimalParameter = 0;
            _bucketTypeDateParameter = DateOnly.MinValue;
            _bucketTypeNextDateParameter = DateOnly.MinValue;
            _notes = string.Empty;
        }
        else
        {
            BucketVersionId = bucketVersion.Id;
            _version = bucketVersion.Version;
            _bucketId = bucketVersion.BucketId;
            ValidFrom = bucketVersion.ValidFrom;
            _bucketTypeParameter = (BucketType)bucketVersion.BucketType;
            _bucketTypeIntParameter = bucketVersion.BucketTypeXParam;
            _bucketTypeDecimalParameter = bucketVersion.BucketTypeYParam;
            _bucketTypeDateParameter = bucketVersion.BucketTypeZParam;
            _bucketTypeNextDateParameter = bucketVersion.BucketTypeZParam;
            _notes = bucketVersion.Notes ?? string.Empty;
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected BucketVersionViewModel(BucketVersionViewModel viewModel) : base(viewModel.ServiceManager)
    {
        BucketVersionId = viewModel.BucketVersionId;
        _version = viewModel.Version;
        _bucketId = viewModel.BucketId;
        ValidFrom = viewModel.ValidFrom;
        _bucketTypeParameter = viewModel.BucketTypeParameter;
        _bucketTypeIntParameter = viewModel.BucketTypeIntParameter;
        _bucketTypeDecimalParameter = viewModel.BucketTypeDecimalParameter;
        _bucketTypeDateParameter = viewModel.BucketTypeDateParameter;
        _bucketTypeNextDateParameter = viewModel.BucketTypeNextDateParameter;
        _notes = viewModel.Notes;
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month</param>
    public static BucketVersionViewModel CreateFromBucket(IServiceManager serviceManager, Bucket bucket, DateOnly yearMonth)
    {
        var bucketVersion = serviceManager.BucketService.GetLatestVersion(bucket.Id, yearMonth);
        return new BucketVersionViewModel(serviceManager, bucketVersion);
    }
    
    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="BucketVersion"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public static BucketVersionViewModel CreateEmpty(IServiceManager serviceManager)
    {
        return new BucketVersionViewModel(serviceManager, null);
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new BucketVersionViewModel(this);
    }

    #endregion

    #region Modification Handler

    /// <summary>
    /// Convert current ViewModel into a corresponding <see cref="IEntity"/> object
    /// </summary>
    /// <returns>Converted ViewModel</returns>
    internal override BucketVersion ConvertToDto()
    {
        return new BucketVersion()
        {
            Id = BucketVersionId,
            Version = Version,
            BucketId = BucketId,
            ValidFrom = ValidFrom,
            BucketType = (int)BucketTypeParameter,
            BucketTypeXParam = BucketTypeIntParameter,
            BucketTypeYParam = BucketTypeDecimalParameter,
            BucketTypeZParam = BucketTypeDateParameter,
            Notes = Notes
        };
    }
    
    #endregion

    #region IEquatable & IComparable Implementation

    public bool Equals(BucketVersionViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            _bucketVersionId.Equals(other._bucketVersionId) && 
            _version == other._version && 
            _bucketId.Equals(other._bucketId) && 
            _validFrom.Equals(other._validFrom) && 
            _bucketTypeParameter == other._bucketTypeParameter && 
            _bucketTypeIntParameter == other._bucketTypeIntParameter && 
            _bucketTypeDecimalParameter == other._bucketTypeDecimalParameter && 
            _bucketTypeDateParameter.Equals(other._bucketTypeDateParameter) && 
            _bucketTypeNextDateParameter.Equals(other._bucketTypeNextDateParameter) && 
            _notes == other._notes;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BucketVersionViewModel)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_bucketVersionId);
        hashCode.Add(_version);
        hashCode.Add(_bucketId);
        hashCode.Add(_validFrom);
        hashCode.Add((int)_bucketTypeParameter);
        hashCode.Add(_bucketTypeIntParameter);
        hashCode.Add(_bucketTypeDecimalParameter);
        hashCode.Add(_bucketTypeDateParameter);
        hashCode.Add(_bucketTypeNextDateParameter);
        hashCode.Add(_notes);
        return hashCode.ToHashCode();
    }
    
    public int CompareTo(BucketVersionViewModel? other)
    {
        return _version.CompareTo(other?.Version);
    }

    public override string ToString() => Version.ToString();

    #endregion
}