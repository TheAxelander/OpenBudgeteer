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
                RuleSets.Clear();
                var currentYearMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
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
        if (ruleSetViewModel.TargetBucket.BucketId == Guid.Empty) return new(false, "No Target Bucket selected.");
        if (ruleSetViewModel.Priority <= 0) return new(false, "Priority should be a positive number.");
        return new(true);
    }
}
