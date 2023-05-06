using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels;

public class RuleSetViewModelItem : ViewModelBase
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

    private ObservableCollection<MappingRuleViewModelItem> _mappingRules;
    /// <summary>
    /// Collection of MappingRules assigned to this RuleSet
    /// </summary>
    public ObservableCollection<MappingRuleViewModelItem> MappingRules
    {
        get => _mappingRules;
        set => Set(ref _mappingRules, value);
    }

    private ObservableCollection<Bucket> _availableBuckets;
    /// <summary>
    /// Helper collection to list all existing Buckets
    /// </summary>
    public ObservableCollection<Bucket> AvailableBuckets
    {
        get => _availableBuckets;
        set => Set(ref _availableBuckets, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;
    private RuleSetViewModelItem _oldRuleSetViewModelItem;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public RuleSetViewModelItem(DbContextOptions<DatabaseContext> dbOptions)
    {
        MappingRules = new ObservableCollection<MappingRuleViewModelItem>();
        AvailableBuckets = new ObservableCollection<Bucket>();
        RuleSet = new BucketRuleSet();
        TargetBucket = new Bucket();
        _dbOptions = dbOptions;
        AvailableBuckets.Add(new Bucket
        {
            BucketId = Guid.Empty,
            BucketGroupId = Guid.Empty,
            Name = "No Selection"
        });
        using var dbContext = new DatabaseContext(_dbOptions);
        AvailableBuckets.Add(dbContext.Bucket.First(i =>
            i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000001")));
        AvailableBuckets.Add(dbContext.Bucket.First(i =>
            i.BucketId == Guid.Parse("00000000-0000-0000-0000-000000000002")));

        var query = dbContext.Bucket
            .Where(i => 
                i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i.BucketId != Guid.Parse("00000000-0000-0000-0000-000000000002") &&
                !i.IsInactive)
            .OrderBy(i => i.Name);

        foreach (var availableBucket in query.ToList())
        {
            AvailableBuckets.Add(availableBucket);
        }
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="BucketRuleSet"/>
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    /// <param name="bucketRuleSet">RuleSet instance</param>
    public RuleSetViewModelItem(DbContextOptions<DatabaseContext> dbOptions, BucketRuleSet bucketRuleSet) :
        this(dbOptions)
    {
        // Make a copy of the object to prevent any double Bindings
        RuleSet = new BucketRuleSet()
        {
            BucketRuleSetId = bucketRuleSet.BucketRuleSetId,
            Name = bucketRuleSet.Name,
            Priority = bucketRuleSet.Priority,
            TargetBucketId = bucketRuleSet.TargetBucketId
        };
        using var dbContext = new DatabaseContext(_dbOptions);
        TargetBucket = dbContext.Bucket.FirstOrDefault(i => i.BucketId == bucketRuleSet.TargetBucketId);
        foreach (var mappingRule in dbContext.MappingRule.Where(i => i.BucketRuleSetId == bucketRuleSet.BucketRuleSetId))
        {
            MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, mappingRule));
        }
    }

    /// <summary>
    /// Helper method to start modification process
    /// </summary>
    public void StartModification()
    {
        _oldRuleSetViewModelItem = new RuleSetViewModelItem(_dbOptions, RuleSet);
        InModification = true;
    }

    /// <summary>
    /// Stops modification process and restores old values
    /// </summary>
    public void CancelModification()
    {
        RuleSet = _oldRuleSetViewModelItem.RuleSet;
        MappingRules = _oldRuleSetViewModelItem.MappingRules;
        InModification = false;
        _oldRuleSetViewModelItem = null;
    }

    /// <summary>
    /// Creates an initial <see cref="MappingRuleViewModelItem"/> and adds it to the <see cref="MappingRules"/>
    /// </summary>
    public void AddEmptyMappingRule()
    {
        MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, new MappingRule()
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
        using var dbContext = new DatabaseContext(_dbOptions);
        using var dbTransaction = dbContext.Database.BeginTransaction();
        try
        {
            if (RuleSet.BucketRuleSetId == Guid.Empty)
            {
                // CREATE
                if (dbContext.CreateBucketRuleSet(RuleSet) == 0)
                    throw new Exception("Rule could not be created in database.");
                foreach (var mappingRule in MappingRules)
                {
                    mappingRule.MappingRule.BucketRuleSetId = RuleSet.BucketRuleSetId;
                }
            }
            else
            {
                // UPDATE
                dbContext.DeleteMappingRules(dbContext.MappingRule.Where(i =>
                    i.BucketRuleSetId == RuleSet.BucketRuleSetId));

                dbContext.UpdateBucketRuleSet(RuleSet);
                foreach (var mappingRule in MappingRules)
                {
                    mappingRule.GenerateRuleOutput();
                }
            }

            foreach (var mappingRuleViewModelItem in MappingRules)
            {
                mappingRuleViewModelItem.MappingRule.MappingRuleId = Guid.Empty;
            }
            dbContext.CreateMappingRules(MappingRules
                .Select(i => i.MappingRule)
                .ToList());
                    
            dbTransaction.Commit();
            _oldRuleSetViewModelItem = null;
            InModification = false;

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            dbTransaction.Rollback();
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes passed MappingRule from the collection 
    /// </summary>
    /// <param name="mappingRule">MappingRule that needs to be removed</param>
    public void DeleteMappingRule(MappingRuleViewModelItem mappingRule)
    {
        //Note: Doesn't require any database updates as this will be done during CreateUpdateRuleSetItem
        MappingRules.Remove(mappingRule);
        if (MappingRules.Count == 0) AddEmptyMappingRule();
    }
}
