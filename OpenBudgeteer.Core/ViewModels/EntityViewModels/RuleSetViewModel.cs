using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class RuleSetViewModel : ViewModelBase
{
    private BucketRuleSet _ruleSet;
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public BucketRuleSet RuleSet
    {
        get => _ruleSet;
        set => Set(ref _ruleSet, value);
    }

    private Bucket _targetBucket;
    /// <summary>
    /// Bucket to which this RuleSet applies
    /// </summary>
    public Bucket TargetBucket
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
    public readonly ObservableCollection<Bucket> AvailableBuckets;

    private RuleSetViewModel? _oldRuleSetViewModelItem;

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketRuleSet"/>
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="availableBuckets">List of all available <see cref="Bucket"/> from database. (Use a cached list here)</param>
    /// <param name="bucketRuleSet">RuleSet instance</param>
    protected RuleSetViewModel(IServiceManager serviceManager, IEnumerable<Bucket> availableBuckets, 
        BucketRuleSet? bucketRuleSet) : base(serviceManager)
    {
        _mappingRules = new ObservableCollection<MappingRuleViewModel>();
        
        // Handle Buckets
        AvailableBuckets = new ObservableCollection<Bucket>();
        foreach (var availableBucket in availableBuckets)
        {
            AvailableBuckets.Add(availableBucket);
        }
        
        // Handle RuleSet
        if (bucketRuleSet == null)
        {
            // Create empty RuleSet
            var noSelectBucket = new Bucket
            {
                Id = Guid.Empty,
                BucketGroupId = Guid.Empty,
                Name = "No Selection"
            };
            AvailableBuckets.Add(noSelectBucket);
            _ruleSet = new();
            _targetBucket = noSelectBucket;
        }
        else
        {
            // Make a copy of the object to prevent any double Bindings
            _ruleSet = new ()
            {
                Id = bucketRuleSet.Id,
                Name = bucketRuleSet.Name,
                Priority = bucketRuleSet.Priority,
                TargetBucketId = bucketRuleSet.TargetBucketId,
            };
            _targetBucket = bucketRuleSet.TargetBucket;
            
            foreach (var mappingRule in ServiceManager.BucketRuleSetService.GetMappingRules(bucketRuleSet.Id))
            {
                MappingRules.Add(new MappingRuleViewModel(serviceManager, mappingRule));
            }
        }
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

    /// <summary>
    /// Helper method to start modification process
    /// </summary>
    public void StartModification()
    {
        _oldRuleSetViewModelItem = CreateFromRuleSet(ServiceManager, AvailableBuckets, RuleSet);
        InModification = true;
    }

    /// <summary>
    /// Stops modification process and restores old values
    /// </summary>
    public void CancelModification()
    {
        RuleSet = _oldRuleSetViewModelItem!.RuleSet;
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
    /// Creates or updates records in the database based on <see cref="RuleSet"/> and <see cref="MappingRules"/> objects
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateUpdateRuleSetItem()
    {
        try
        {
            var mappingRules = MappingRules.Select(mappingRule => mappingRule.MappingRule).ToList();
            
            if (RuleSet.Id == Guid.Empty)
            {
                // CREATE
                ServiceManager.BucketRuleSetService.Create(RuleSet, mappingRules);
            }
            else
            {
                // UPDATE
                ServiceManager.BucketRuleSetService.Update(RuleSet, mappingRules);
            }
                    
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
}