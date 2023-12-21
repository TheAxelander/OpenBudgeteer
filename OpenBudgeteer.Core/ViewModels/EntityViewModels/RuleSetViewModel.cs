using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class RuleSetViewModel : BaseEntityViewModel<BucketRuleSet>
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
    
    private Guid _targetBucketId;
    /// <summary>
    /// Database Id of the Bucket which will be used once the BucketRuleSet applies
    /// </summary>
    public Guid TargetBucketId 
    { 
        get => _targetBucketId;
        set => Set(ref _targetBucketId, value);
    }
    
    private string _targetBucketName;
    /// <summary>
    /// Name of the Bucket
    /// </summary>
    public string TargetBucketName
    {
        get => _targetBucketName;
        set => Set(ref _targetBucketName, value);
    }
    
    private string _targetBucketColorCode;
    /// <summary>
    /// Name of the background color based from <see cref="Color"/>
    /// </summary>
    public string TargetBucketColorCode 
    { 
        get => _targetBucketColorCode;
        set => Set(ref _targetBucketColorCode, value);
    }
    
    /// <summary>
    /// Background <see cref="Color"/> of the Bucket 
    /// </summary>
    public Color TargetBucketColor => string.IsNullOrEmpty(TargetBucketColorCode) ? Color.LightGray : Color.FromName(TargetBucketColorCode);
    
    private string _targetBucketTextColorCode;
    /// <summary>
    /// Name of the text color based from <see cref="Color"/>
    /// </summary>
    public string TargetBucketTextColorCode 
    { 
        get => _targetBucketTextColorCode;
        set => Set(ref _targetBucketTextColorCode, value);
    }
    
    /// <summary>
    /// Text <see cref="Color"/> of the Bucket 
    /// </summary>
    public Color TargetBucketTextColor => string.IsNullOrEmpty(TargetBucketTextColorCode) ? Color.Black : Color.FromName(TargetBucketTextColorCode);
    
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
        if (bucketRuleSet == null)
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
            _targetBucketId = noSelectBucket.Id;
            _targetBucketName = noSelectBucket.Name;
            _targetBucketColorCode = string.Empty;
            _targetBucketTextColorCode = string.Empty;
        }
        else
        {
            BucketRuleSetId = bucketRuleSet.Id;
            _name = bucketRuleSet.Name ?? string.Empty;
            _priority = bucketRuleSet.Priority;
            _targetBucketId = bucketRuleSet.TargetBucketId;
            _targetBucketName = bucketRuleSet.TargetBucket.Name ?? string.Empty;
            _targetBucketColorCode = bucketRuleSet.TargetBucket.ColorCode ?? string.Empty;
            _targetBucketTextColorCode = bucketRuleSet.TargetBucket.TextColorCode ?? string.Empty;
            
            foreach (var mappingRule in bucketRuleSet.MappingRules)
            {
                MappingRules.Add(new MappingRuleViewModel(serviceManager, mappingRule));
            }
        }
    }

    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected RuleSetViewModel(RuleSetViewModel viewModel) : base(viewModel.ServiceManager)
    {
        // Handle Mapping Rules
        _mappingRules = new ObservableCollection<MappingRuleViewModel>();
        foreach (var mappingRule in MappingRules)
        {
            _mappingRules.Add(mappingRule);
        }
        
        // Handle Buckets
        _availableBuckets = new ObservableCollection<Bucket>();
        foreach (var availableBucket in viewModel._availableBuckets)
        {
            _availableBuckets.Add(availableBucket);
        }
        
        // Handle RuleSet
        BucketRuleSetId = viewModel.BucketRuleSetId;
        _name = viewModel.Name;
        _priority = viewModel.Priority;
        _targetBucketId = viewModel.TargetBucketId;
        _targetBucketName = viewModel.TargetBucketName;
        _targetBucketColorCode = viewModel.TargetBucketColorCode;
        _targetBucketTextColorCode = viewModel.TargetBucketTextColorCode;
    }

    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="BucketRuleSet"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public static RuleSetViewModel CreateEmpty(IServiceManager serviceManager)
    {
        var currentYearMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
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
    
    #endregion
    
    #region Modification Handler
    
    internal override BucketRuleSet ConvertToDto()
    {
        return new BucketRuleSet()
        {
            Id = BucketRuleSetId,
            Name = Name,
            Priority = Priority,
            TargetBucketId = TargetBucketId
        };
    }
    
    private BucketRuleSet ConvertToDtoFull()
    {
        var result = ConvertToDto();
        result.TargetBucket = new Bucket()
        {
            Id = TargetBucketId,
            Name = TargetBucketName,
            ColorCode = TargetBucketColorCode,
            TextColorCode = TargetBucketTextColorCode
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
    /// Updates ViewModel data based on ViewModel data
    /// </summary>
    /// <param name="bucketViewModel">Newly selected Bucket</param>
    public void UpdateSelectedBucket(BucketViewModel bucketViewModel)
    {
        TargetBucketId = bucketViewModel.BucketId;
        TargetBucketName = bucketViewModel.Name;
        TargetBucketColorCode = bucketViewModel.ColorCode;
        TargetBucketTextColorCode = bucketViewModel.TextColorCode;
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
        TargetBucketId = _oldRuleSetViewModelItem!.TargetBucketId;
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
            ComparisionField = 1,
            ComparisionType = 1
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
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }
    
    #endregion
}