using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;

namespace OpenBudgeteer.Core.ViewModels.ItemViewModels
{
    public class RuleSetViewModelItem : ViewModelBase
    {
        private BucketRuleSet _ruleSet;
        public BucketRuleSet RuleSet
        {
            get => _ruleSet;
            set => Set(ref _ruleSet, value);
        }

        private Bucket _targetBucket;
        public Bucket TargetBucket
        {
            get => _targetBucket;
            set => Set(ref _targetBucket, value);
        }


        private bool _inModification;
        public bool InModification
        {
            get => _inModification;
            set => Set(ref _inModification, value);
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set => Set(ref _isHovered, value);
        }

        private ObservableCollection<MappingRuleViewModelItem> _mappingRules;
        public ObservableCollection<MappingRuleViewModelItem> MappingRules
        {
            get => _mappingRules;
            set => Set(ref _mappingRules, value);
        }

        private ObservableCollection<Bucket> _availableBuckets;
        public ObservableCollection<Bucket> AvailableBuckets
        {
            get => _availableBuckets;
            set => Set(ref _availableBuckets, value);
        }

        private readonly DbContextOptions<DatabaseContext> _dbOptions;
        private RuleSetViewModelItem _oldRuleSetViewModelItem;

        public RuleSetViewModelItem()
        {
            MappingRules = new ObservableCollection<MappingRuleViewModelItem>();
            AvailableBuckets = new ObservableCollection<Bucket>();
            RuleSet = new BucketRuleSet();
            TargetBucket = new Bucket();
        }

        public RuleSetViewModelItem(DbContextOptions<DatabaseContext> dbOptions) : this()
        {
            _dbOptions = dbOptions;
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                foreach (var bucket in dbContext.Bucket)
                {
                    AvailableBuckets.Add(bucket);
                }
            }
        }

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
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                TargetBucket = dbContext.Bucket.FirstOrDefault(i => i.BucketId == bucketRuleSet.TargetBucketId);
                foreach (var mappingRule in dbContext.MappingRule.Where(i => i.BucketRuleSetId == bucketRuleSet.BucketRuleSetId))
                {
                    MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, mappingRule));
                }
            }
        }

        public void StartModification()
        {
            _oldRuleSetViewModelItem = new RuleSetViewModelItem(_dbOptions, RuleSet);
            InModification = true;
        }

        public void CancelModification()
        {
            RuleSet = _oldRuleSetViewModelItem.RuleSet;
            MappingRules = _oldRuleSetViewModelItem.MappingRules;
            InModification = false;
            _oldRuleSetViewModelItem = null;
        }

        public void AddEmptyMappingRule()
        {
            MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, new MappingRule()));
        }

        public Tuple<bool, string> CreateUpdateRuleSetItem()
        {
            var result = new Tuple<bool, string>(true, string.Empty);
            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var dbTransaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (RuleSet.BucketRuleSetId == 0)
                        {
                            if (dbContext.CreateBucketRuleSet(RuleSet) == 0)
                                throw new Exception("Rule could not be created in database.");
                            foreach (var mappingRule in MappingRules)
                            {
                                mappingRule.MappingRule.BucketRuleSetId = RuleSet.BucketRuleSetId;
                            }
                        }
                        else
                        {
                            dbContext.DeleteMappingRules(dbContext.MappingRule.Where(i =>
                                i.BucketRuleSetId == RuleSet.BucketRuleSetId));

                            dbContext.UpdateBucketRuleSet(RuleSet);
                        }

                        dbContext.CreateMappingRules(MappingRules.Select(i => i.MappingRule).ToList());
                        
                        dbTransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        result = new Tuple<bool, string>(false, $"Errors during database update: {e.Message}");
                    }
                }
                
            }
            _oldRuleSetViewModelItem = null;
            InModification = false;

            return result;
        }

        public void DeleteMappingRule(MappingRuleViewModelItem mappingRule)
        {
            MappingRules.Remove(mappingRule);
            if (MappingRules.Count == 0) AddEmptyMappingRule();
        }
    }
}
