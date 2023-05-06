using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;

namespace OpenBudgeteer.Core.ViewModels;

public class RulesViewModel : ViewModelBase
{
    private RuleSetViewModelItem _newRuleSet;
    /// <summary>
    /// Helper property to handle setup of a new <see cref="BucketRuleSet"/>
    /// </summary>
    public RuleSetViewModelItem NewRuleSet
    {
        get => _newRuleSet;
        set => Set(ref _newRuleSet, value);
    }

    private ObservableCollection<RuleSetViewModelItem> _ruleSets;
    /// <summary>
    /// Collection of all <see cref="BucketRuleSet"/> from the database
    /// </summary>
    public ObservableCollection<RuleSetViewModelItem> RuleSets
    {
        get => _ruleSets;
        set => Set(ref _ruleSets, value);
    }

    private readonly DbContextOptions<DatabaseContext> _dbOptions;

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="dbOptions">Options to connect to a database</param>
    public RulesViewModel(DbContextOptions<DatabaseContext> dbOptions)
    {
        _dbOptions = dbOptions;
        RuleSets = new ObservableCollection<RuleSetViewModelItem>();
        ResetNewRuleSet();
    }
    
    /// <summary>
    /// Initialize ViewModel and load data from database
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public async Task<ViewModelOperationResult> LoadDataAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                ResetNewRuleSet();
                RuleSets.Clear();
                using (var dbContext = new DatabaseContext(_dbOptions))
                {
                    foreach (var bucketRuleSet in dbContext.BucketRuleSet.OrderBy(i => i.Priority))
                    {
                        RuleSets.Add(new RuleSetViewModelItem(_dbOptions, bucketRuleSet));
                    }
                }

                return new ViewModelOperationResult(true);
            }
            catch (Exception e)
            {
                return new ViewModelOperationResult(false, $"Error during loading: {e.Message}");
            }
        });
    }

    /// <summary>
    /// Starts creation process based on <see cref="NewRuleSet"/>
    /// </summary>
    /// <remarks>Triggers <see cref="ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateNewRuleSet()
    {
        var validationResult = ValidateRuleSet(NewRuleSet);
        if (!validationResult.IsSuccessful) return validationResult; 
        NewRuleSet.RuleSet.BucketRuleSetId = Guid.Empty;
        var result = NewRuleSet.CreateUpdateRuleSetItem();
        if (!result.IsSuccessful) return result;
        ResetNewRuleSet();

        return new ViewModelOperationResult(true, true);
    }

    /// <summary>
    /// Helper method to reset values of <see cref="NewRuleSet"/>
    /// </summary>
    public void ResetNewRuleSet()
    {
        NewRuleSet = new RuleSetViewModelItem(_dbOptions)
        {
            TargetBucket = new Bucket(){ Name = "No Selection" }
        };
        // Defaults required because if initial selection in UI will not be updated by User
        // then binding will not update these properties
        NewRuleSet.MappingRules.Add(new MappingRuleViewModelItem(_dbOptions, new MappingRule()
        {
            ComparisionField = 1,
            ComparisionType = 1
        }));
    }

    /// <summary>
    /// Starts Creation or Update process for the passed <see cref="RuleSetViewModelItem"/>
    /// </summary>
    /// <remarks>Updates <see cref="RuleSets"/> collection</remarks>
    /// <param name="ruleSet">Instance that needs to be created or updated</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveRuleSetItem(RuleSetViewModelItem ruleSet)
    {
        var validationResult = ValidateRuleSet(ruleSet);
        if (!validationResult.IsSuccessful) return validationResult; 
        
        var result = ruleSet.CreateUpdateRuleSetItem();
        if (!result.IsSuccessful) return result;
        RuleSets = new ObservableCollection<RuleSetViewModelItem>(RuleSets.OrderBy(i => i.RuleSet.Priority));

        return result;
    }

    private ViewModelOperationResult ValidateRuleSet(RuleSetViewModelItem ruleSetViewModelItem)
    {
        if (ruleSetViewModelItem.TargetBucket.BucketId == Guid.Empty) return new(false, "No Target Bucket selected.");
        if (ruleSetViewModelItem.RuleSet.Priority <= 0) return new(false, "Priority should be a positive number.");
        return new(true);
    }

    /// <summary>
    /// Starts Deletion process for the passed <see cref="RuleSetViewModelItem"/> including all its <see cref="MappingRule"/>
    /// </summary>
    /// <remarks>Updates <see cref="RuleSets"/> collection</remarks>
    /// <param name="ruleSet">Instance that needs to be deleted</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteRuleSetItem(RuleSetViewModelItem ruleSet)
    {
        using var dbContext = new DatabaseContext(_dbOptions);
        using var dbTransaction = dbContext.Database.BeginTransaction();
        try
        {
            dbContext.DeleteMappingRules(dbContext.MappingRule
                .Where(i => i.BucketRuleSetId == ruleSet.RuleSet.BucketRuleSetId));
            dbContext.DeleteBucketRuleSet(ruleSet.RuleSet);
            dbTransaction.Commit();
            RuleSets.Remove(ruleSet);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            dbTransaction.Rollback();
            return new ViewModelOperationResult(false, $"Errors during database update: {e.Message}");
        }
    }

    /// <summary>
    /// Helper method to start Modification process for all <see cref="BucketRuleSet"/>
    /// </summary>
    public void EditAllRules()
    {
        foreach (var ruleSet in RuleSets)
        {
            ruleSet.StartModification();
        }
    }

    /// <summary>
    /// Starts the Creation or Update process for all <see cref="BucketRuleSet"/>
    /// </summary>
    /// <remarks>Updates <see cref="RuleSets"/> collection</remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveAllRules()
    {
        using var dbTransaction = new DatabaseContext(_dbOptions).Database.BeginTransaction();
        try
        {
            foreach (var ruleSet in RuleSets)
            {
                var result = ruleSet.CreateUpdateRuleSetItem();
                if (!result.IsSuccessful) throw new Exception(result.Message);
            }
            dbTransaction.Commit();
            RuleSets = new ObservableCollection<RuleSetViewModelItem>(RuleSets.OrderBy(i => i.RuleSet.Priority));

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            dbTransaction.Rollback();
            return new ViewModelOperationResult(false, e.Message);
        }
    }
}
