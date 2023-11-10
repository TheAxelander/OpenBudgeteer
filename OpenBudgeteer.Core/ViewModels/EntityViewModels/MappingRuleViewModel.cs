using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class MappingRuleViewModel : ViewModelBase
{
    /// <summary>
    /// Reference to model object in the database
    /// </summary>
    public readonly MappingRule MappingRule;

    /// <summary>
    /// Helper property to generate a readable output for <see cref="MappingRule"/>
    /// </summary>
    public string RuleOutput => $"{MappingRule.ComparisonFieldOutput} " +
                                $"{MappingRule.ComparisionTypeOutput} " +
                                $"{MappingRule.ComparisionValue}";

    /// <summary>
    /// Initialize ViewModel based an existing <see cref="MappingRule"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="mappingRule">MappingRule instance</param>
    public MappingRuleViewModel(IServiceManager serviceManager, MappingRule mappingRule) 
        : base(serviceManager)
    {
        MappingRule = mappingRule;
    }
}