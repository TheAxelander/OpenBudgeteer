using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class RuleSetViewModel : BaseEntityViewModel<BucketRuleSet>, IEquatable<RuleSetViewModel>, IComparable<RuleSetViewModel>
{
    #region Properties & Fields
    
    /// <summary>
    /// Database Id of the BucketRuleSetId
    /// </summary>
    public readonly Guid BucketRuleSetId;
    
    private int _priority;
    /// <summary>
    /// Priority in which order this BucketRuleSet should apply
    /// </summary>
    public int Priority 
    { 
        get => _priority;
        set => Set(ref _priority, value);
    }
    
    private string _name;
    /// <summary>
    /// Name of the BucketRuleSet
    /// </summary>
    public string Name 
    { 
        get => _name;
        set => Set(ref _name, value);
    }
    
    private BucketViewModel _targetBucket;
    /// <summary>
    /// Reference to the Bucket which will be used once the BucketRuleSet applies
    /// </summary>
    public BucketViewModel TargetBucket 
    { 
        get => _targetBucket;
        set => Set(ref _targetBucket, value);
    }
    
    private bool _inModification;
    /// <summary>
    /// Helper property to check if the RuleSet is currently modified
    /// </summary>
    public bool InModification
    {
        get => _inModification;
        set => Set(ref _inModification, value);
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

    private ObservableCollection<MappingRuleViewModel> _mappingRules;
    /// <summary>
    /// Collection of MappingRules assigned to this RuleSet
    /// </summary>
    public ObservableCollection<MappingRuleViewModel> MappingRules
    {
        get => _mappingRules;
        set => Set(ref _mappingRules, value);
    }

    /// <summary>
    /// Helper collection to list all existing Buckets
    /// </summary>
    private readonly ObservableCollection<Bucket> _availableBuckets;

    private RuleSetViewModel? _oldRuleSetViewModelItem;
    
    #endregion
    
    #region Constructors

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketRuleSet"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="bucketRuleSet">RuleSet instance</param>
    protected RuleSetViewModel(IServiceManager serviceManager, IEnumerable<Bucket> availableBuckets, 
        BucketRuleSet? bucketRuleSet) : base(serviceManager)
    {
        _mappingRules = new ObservableCollection<MappingRuleViewModel>();
        
        // Handle Buckets
        _availableBuckets = new ObservableCollection<Bucket>();
        foreach (var availableBucket in availableBuckets)
        {
            _availableBuckets.Add(availableBucket);
        }
        
        // Handle RuleSet
        if (bucketRuleSet is null)
        {
            // Create empty RuleSet
            var noSelectBucket = new Bucket
            {
                Id = Guid.Empty,
                BucketGroupId = Guid.Empty,
                BucketGroup = new BucketGroup(),
                Name = "No Selection"
            };
            _availableBuckets.Add(noSelectBucket);
            
            BucketRuleSetId = Guid.Empty;
            _name = string.Empty;
            _priority = 0;
            _targetBucket = BucketViewModel.CreateNoSelection(serviceManager);
        }
        else
        {
            BucketRuleSetId = bucketRuleSet.Id;
            _name = bucketRuleSet.Name ?? string.Empty;
            _priority = bucketRuleSet.Priority;
            _targetBucket = BucketViewModel.CreateForListing(serviceManager, bucketRuleSet.TargetBucket);

            if (bucketRuleSet.MappingRules is not null)
            {
                foreach (var mappingRule in bucketRuleSet.MappingRules)
                {
                    MappingRules.Add(new MappingRuleViewModel(serviceManager, mappingRule));
                }
            }
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected RuleSetViewModel(RuleSetViewModel viewModel) : base(viewModel.ServiceManager)
    {
        // Handle RuleSet
        BucketRuleSetId = viewModel.BucketRuleSetId;
        _name = viewModel.Name;
        _priority = viewModel.Priority;
        _targetBucket = (viewModel.TargetBucket.Clone() as BucketViewModel)!;
        _inModification = viewModel.InModification;
        _isHovered = viewModel.IsHovered;
        
        // Handle Mapping Rules
        _mappingRules = new ObservableCollection<MappingRuleViewModel>();
        foreach (var mappingRule in viewModel.MappingRules)
        {
            _mappingRules.Add((MappingRuleViewModel)mappingRule.Clone());
        }
        
        // Handle Buckets
        _availableBuckets = new ObservableCollection<Bucket>();
        foreach (var availableBucket in viewModel._availableBuckets)
        {
            _availableBuckets.Add(availableBucket);
        }
    }

    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="BucketRuleSet"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public static RuleSetViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var currentYearMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var availableBuckets = serviceManager.BucketService.GetActiveBuckets(currentYearMonth).ToList();
        return new RuleSetViewModel(serviceManager, availableBuckets, null);
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketRuleSet"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="bucketRuleSet">RuleSet instance</param>
    public static RuleSetViewModel CreateFromRuleSet(IServiceManager serviceManager,IEnumerable<Bucket> availableBuckets, 
        BucketRuleSet? bucketRuleSet)
    {
        return new RuleSetViewModel(serviceManager, availableBuckets, bucketRuleSet);
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new RuleSetViewModel(this);
    }

    #endregion
    
    #region Modification Handler
    
    internal override BucketRuleSet ConvertToDto()
    {
        return new BucketRuleSet()
        {
            Id = BucketRuleSetId,
            Name = Name,
            Priority = Priority,
            TargetBucketId = TargetBucket.BucketId
        };
    }
    
    private BucketRuleSet ConvertToDtoFull()
    {
        var result = ConvertToDto();
        result.TargetBucket = new Bucket()
        {
            Id = TargetBucket.BucketId,
            Name = TargetBucket.Name,
            ColorCode = TargetBucket.ColorCode,
            TextColorCode = TargetBucket.TextColorCode
        };
        result.MappingRules = new List<MappingRule>();
        foreach (var mappingRuleViewModel in MappingRules)
        {
            result.MappingRules.Add(mappingRuleViewModel.ConvertToDto());
        }
        return result;
    }
    
    private BucketRuleSet ConvertToDtoWithMappingRules()
    {
        var result = ConvertToDto();
        result.MappingRules = new List<MappingRule>();
        foreach (var mappingRuleViewModel in MappingRules)
        {
            result.MappingRules.Add(mappingRuleViewModel.ConvertToDto());
        }

        return result;
    }

    /// <summary>
    /// Helper method to start modification process
    /// </summary>
    public void StartModification()
    {
        _oldRuleSetViewModelItem = new RuleSetViewModel(this);
        InModification = true;
    }

    /// <summary>
    /// Stops modification process and restores old values
    /// </summary>
    public void CancelModification()
    {
        Name = _oldRuleSetViewModelItem!.Name;
        Priority = _oldRuleSetViewModelItem!.Priority;
        TargetBucket = _oldRuleSetViewModelItem!.TargetBucket;
        MappingRules = _oldRuleSetViewModelItem.MappingRules;
        InModification = false;
        _oldRuleSetViewModelItem = null;
    }

    /// <summary>
    /// Creates an initial <see cref="MappingRuleViewModel"/> and adds it to the <see cref="MappingRules"/>
    /// </summary>
    public void AddEmptyMappingRule()
    {
        MappingRules.Add(new MappingRuleViewModel(ServiceManager, new MappingRule()
        {
            ComparisonField = 1,
            ComparisonType = 1
        }));
    }

    /// <summary>
    /// Creates or updates records in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateUpdateRuleSetItem()
    {
        try
        {
            if (BucketRuleSetId == Guid.Empty)
                ServiceManager.BucketRuleSetService.Create(ConvertToDtoWithMappingRules());
            else
                ServiceManager.BucketRuleSetService.Update(ConvertToDtoWithMappingRules());
                    
            _oldRuleSetViewModelItem = null;
            InModification = false;

            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }

    /// <summary>
    /// Deletes passed MappingRule from the collection 
    /// </summary>
    /// <param name="mappingRule">MappingRule that needs to be removed</param>
    public void DeleteMappingRule(MappingRuleViewModel mappingRule)
    {
        //Note: Doesn't require any database updates as this will be done during CreateUpdateRuleSetItem
        MappingRules.Remove(mappingRule);
        if (MappingRules.Count == 0) AddEmptyMappingRule();
    }
    
    /// <summary>
    /// Deletes records in the database based on ViewModel data
    /// </summary>
    /// <remarks>Deletes also all <see cref="MappingRule"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteRuleSet()
    {
        try
        {
            ServiceManager.BucketRuleSetService.Delete(BucketRuleSetId);
            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    #endregion

    #region IEquatable & IComparable Implementation

    public bool Equals(RuleSetViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            BucketRuleSetId.Equals(other.BucketRuleSetId) && 
            _priority == other._priority && 
            _name == other._name && 
            TargetBucket.Equals(other.TargetBucket) &&
            _mappingRules.Equals(other._mappingRules);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RuleSetViewModel)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(BucketRuleSetId);
        hashCode.Add(_priority);
        hashCode.Add(_name);
        hashCode.Add(TargetBucket);
        hashCode.Add(_mappingRules);
        return hashCode.ToHashCode();
    }
    
    public int CompareTo(RuleSetViewModel? other)
    {
        return string.Compare(_name, other?.Name, StringComparison.Ordinal);
    }

    public override string ToString() => Name;

    #endregion
}