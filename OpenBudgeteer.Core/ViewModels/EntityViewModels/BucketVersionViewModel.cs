using System;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class BucketVersionViewModel : BaseEntityViewModel<BucketVersion>
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
    
    private DateTime _validFrom;
    /// <summary>
    /// Date from which this BucketVersion applies
    /// </summary>
    public DateTime ValidFrom 
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

    private DateTime _bucketTypeDateParameter;
    /// <summary>
    /// Date based parameter of the Bucket type
    /// </summary>
    public DateTime BucketTypeDateParameter
    {
        get => _bucketTypeDateParameter;
        set
        {
            if (Set(ref _bucketTypeDateParameter, value)) HasModification = true;
        }
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
    
    #endregion

    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketVersion"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucketVersion">BucketVersion instance</param>
    protected BucketVersionViewModel(IServiceManager serviceManager, BucketVersion? bucketVersion) : base(serviceManager)
    {
        if (bucketVersion == null)
        {
            BucketVersionId = Guid.Empty;
            _version = 0;
            //BucketId = bucketVersion.BucketId;    Will be set in Database creation phase
            //ValidFrom = bucketVersion.ValidFrom;  Will be set in Database creation phase
            _bucketTypeParameter = BucketType.StandardBucket;
            _bucketTypeIntParameter = 0;
            _bucketTypeDecimalParameter = 0;
            _bucketTypeDateParameter = DateTime.MinValue;
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
        _notes = viewModel.Notes;
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="Bucket"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="bucket">Bucket instance</param>
    /// <param name="yearMonth">Current month</param>
    public static BucketVersionViewModel CreateFromBucket(IServiceManager serviceManager, Bucket bucket, DateTime yearMonth)
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
}