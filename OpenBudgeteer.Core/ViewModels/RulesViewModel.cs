using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Models;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels
{
    public class RulesViewModel : ViewModelBase
    {
        private RuleSetViewModelItem _newRuleSet;
        public RuleSetViewModelItem NewRuleSet
        {
            get => _newRuleSet;
            set => Set(ref _newRuleSet, value);
        }

        private ObservableCollection<RuleSetViewModelItem> _ruleSets;
        public ObservableCollection<RuleSetViewModelItem> RuleSets
        {
            get => _ruleSets;
            set => Set(ref _ruleSets, value);
        }

        public event ViewModelReloadRequiredHandler ViewModelReloadRequired;
        public delegate void ViewModelReloadRequiredHandler(ViewModelBase sender);

        private readonly DbContextOptions<DatabaseContext> _dbOptions;

        public RulesViewModel()
        {
            RuleSets = new ObservableCollection<RuleSetViewModelItem>();
        }

        public RulesViewModel(DbContextOptions<DatabaseContext> dbOptions) : this()
        {
            _dbOptions = dbOptions;
            ResetNewRuleSet();
        }
        
        public async Task<Tuple<bool, string>> LoadDataAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    RuleSets.Clear();
                    using (var dbContext = new DatabaseContext(_dbOptions))
                    {
                        foreach (var bucketRuleSet in dbContext.BucketRuleSet.OrderBy(i => i.Priority))
                        {
                            RuleSets.Add(new RuleSetViewModelItem(_dbOptions, bucketRuleSet));
                        }
                    }
                }
                catch (Exception e)
                {
                    return new Tuple<bool, string>(false, $"Error during loading: {e.Message}");
                }
                return new Tuple<bool, string>(true, string.Empty);
                
            });
        }

        public Tuple<bool, string> CreateNewRuleSet()
        {
            NewRuleSet.RuleSet.BucketRuleSetId = 0;
            var (result, message) = NewRuleSet.CreateUpdateRuleSetItem();
            if (!result)
            {
                return new Tuple<bool, string>(false, message);
            }
            ResetNewRuleSet();
            ViewModelReloadRequired?.Invoke(this);

            return new Tuple<bool, string>(true, string.Empty);
        }

        public void ResetNewRuleSet()
        {
            NewRuleSet = new RuleSetViewModelItem(_dbOptions);
            NewRuleSet.MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, new MappingRule()));
        }

        public Tuple<bool, string> SaveRuleSetItem(RuleSetViewModelItem ruleSet)
        {
            var result = ruleSet.CreateUpdateRuleSetItem();
            if (!result.Item1) return result;
            RuleSets = new ObservableCollection<RuleSetViewModelItem>(RuleSets.OrderBy(i => i.RuleSet.Priority));

            return result;
        }

        public Tuple<bool, string> DeleteRuleSetItem(RuleSetViewModelItem ruleSet)
        {
            var result = new Tuple<bool, string>(true, string.Empty);

            using (var dbContext = new DatabaseContext(_dbOptions))
            {
                using (var dbTransaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        dbContext.DeleteMappingRules(dbContext.MappingRule.Where(i =>
                            i.BucketRuleSetId == ruleSet.RuleSet.BucketRuleSetId));
                        dbContext.DeleteBucketRuleSet(ruleSet.RuleSet);
                        dbTransaction.Commit();
                        RuleSets.Remove(ruleSet);
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        return new Tuple<bool, string>(false, $"Errors during database update: {e.Message}");
                    }
                }

            }

            return result;
        }

        public void EditAllRules()
        {
            foreach (var ruleSet in RuleSets)
            {
                ruleSet.StartModification();
            }
        }

        public Tuple<bool, string> SaveAllRules()
        {
            using (var dbTransaction = new DatabaseContext(_dbOptions).Database.BeginTransaction())
            {
                try
                {
                    foreach (var ruleSet in RuleSets)
                    {
                        (bool success, string message) = ruleSet.CreateUpdateRuleSetItem();
                        if (!success) throw new Exception(message);
                    }
                    dbTransaction.Commit();
                    RuleSets = new ObservableCollection<RuleSetViewModelItem>(RuleSets.OrderBy(i => i.RuleSet.Priority));
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    return new Tuple<bool, string>(false, e.Message);
                }
            }
            return new Tuple<bool, string>(true, string.Empty);
        }

        public void CancelAllRules()
        {
            ViewModelReloadRequired?.Invoke(this);
        }
    }
}
