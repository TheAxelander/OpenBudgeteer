using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;
using OpenBudgeteer.Core.Common.Extensions;

namespace OpenBudgeteer.Blazor.Common.CustomMudFilter;

public class MappingRuleMudFilter<T> : ICustomMudFilter<T>
{
    public MappingRuleComparisonField SelectedComparisonField { get; set; }
    public string ComparisionValue { get; set; } = string.Empty;
    
    public FilterDefinition<T> FilterDefinition { get; }

    public List<MappingRuleComparisonField> AvailableComparisonFields => 
        Enum.GetValues<MappingRuleComparisonField>().ToList();

    public MappingRuleMudFilter(FilterDefinition<T> filterDefinition)
    {
        FilterDefinition = filterDefinition;
    }
    
    public async Task ApplyFilterOnContextAsync(FilterContext<T> filterContext)
    {
        await filterContext.Actions.ApplyFilterAsync(FilterDefinition);
    }

    public async Task ResetFilterOnContextAsync(FilterContext<T> filterContext)
    {
        ResetFilter();
        await filterContext.Actions.ClearFilterAsync(FilterDefinition);
    }

    public void ResetFilter()
    {
        SelectedComparisonField = MappingRuleComparisonField.Any;
        ComparisionValue = string.Empty;
    }
}