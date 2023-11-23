using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.ViewModels.PageViewModels;

public class RulesPageViewModel : ViewModelBase
{
    private RuleSetViewModel? _newRuleSet;
    /// <summary>
    /// Helper property to handle setup of a new <see cref="BucketRuleSet"/>
    /// </summary>
    public RuleSetViewModel? NewRuleSet
    {
        get => _newRuleSet;
        private set => Set(ref _newRuleSet, value);
    }

    private ObservableCollection<RuleSetViewModel> _ruleSets;
    /// <summary>
    /// Collection of all <see cref="BucketRuleSet"/> from the database
    /// </summary>
    public ObservableCollection<RuleSetViewModel> RuleSets
    {
        get => _ruleSets;
        private set => Set(ref _ruleSets, value);
    }

    /// <summary>
    /// Basic constructor
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    public RulesPageViewModel(IServiceManager serviceManager) : base(serviceManager)
    {
        _ruleSets = new ObservableCollection<RuleSetViewModel>();
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
                var currentYearMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var availableBuckets = ServiceManager.BucketService.GetActiveBuckets(currentYearMonth).ToList();
                foreach (var bucketRuleSet in ServiceManager.BucketRuleSetService.GetAll().ToList())
                {
                    RuleSets.Add(RuleSetViewModel.CreateFromRuleSet(ServiceManager, availableBuckets, bucketRuleSet));
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
    /// <remarks>Triggers <see cref="ViewModelOperationResult.ViewModelReloadRequired"/></remarks>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateNewRuleSet()
    {
        try
        {
            if (NewRuleSet == null) throw new Exception("New RuleSet has not been initialized");
            var validationResult = ValidateRuleSet(NewRuleSet);
            if (!validationResult.IsSuccessful) return validationResult; 
            var result = NewRuleSet.CreateUpdateRuleSetItem();
            if (!result.IsSuccessful) return result;
            ResetNewRuleSet();

            return new ViewModelOperationResult(true, true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message); 
        }
    }

    /// <summary>
    /// Helper method to reset values of <see cref="NewRuleSet"/>
    /// </summary>
    public void ResetNewRuleSet()
    {
        NewRuleSet = RuleSetViewModel.CreateEmpty(ServiceManager);
        // Defaults required because if initial selection in UI will not be updated by User
        // then binding will not update these properties
        NewRuleSet.MappingRules.Add(new MappingRuleViewModel(ServiceManager, new MappingRule()
        {
            ComparisionField = 1,
            ComparisionType = 1
        }));
    }

    /// <summary>
    /// Starts Creation or Update process for the passed <see cref="RuleSetViewModel"/>
    /// </summary>
    /// <remarks>Updates <see cref="RuleSets"/> collection</remarks>
    /// <param name="ruleSet">Instance that needs to be created or updated</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveRuleSetItem(RuleSetViewModel ruleSet)
    {
        var validationResult = ValidateRuleSet(ruleSet);
        if (!validationResult.IsSuccessful) return validationResult; 
        
        var result = ruleSet.CreateUpdateRuleSetItem();
        if (!result.IsSuccessful) return result;
        RuleSets = new ObservableCollection<RuleSetViewModel>(RuleSets.OrderBy(i => i.Priority));

        return result;
    }

    private ViewModelOperationResult ValidateRuleSet(RuleSetViewModel ruleSetViewModel)
    {
        if (ruleSetViewModel.TargetBucketId == Guid.Empty) return new(false, "No Target Bucket selected.");
        if (ruleSetViewModel.Priority <= 0) return new(false, "Priority should be a positive number.");
        return new(true);
    }

    /// <summary>
    /// Starts Deletion process for the passed <see cref="RuleSetViewModel"/> including all its <see cref="MappingRule"/>
    /// </summary>
    /// <remarks>Updates <see cref="RuleSets"/> collection</remarks>
    /// <param name="ruleSet">Instance that needs to be deleted</param>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteRuleSetItem(RuleSetViewModel ruleSet)
    {
        try
        {
            var result = ruleSet.DeleteRuleSet();
            if (!result.IsSuccessful) throw new Exception(result.Message);
            RuleSets.Remove(ruleSet);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
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
        try
        {
            foreach (var ruleSet in RuleSets)
            {
                var result = ruleSet.CreateUpdateRuleSetItem();
                if (!result.IsSuccessful) throw new Exception(result.Message);
            }
            RuleSets = new ObservableCollection<RuleSetViewModel>(RuleSets.OrderBy(i => i.Priority));

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, e.Message);
        }
    }
}
