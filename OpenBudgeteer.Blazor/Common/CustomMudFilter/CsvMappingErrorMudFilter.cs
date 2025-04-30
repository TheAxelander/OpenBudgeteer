using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;

namespace OpenBudgeteer.Blazor.Common.CustomMudFilter;

public class CsvMappingErrorMudFilter<T> : ICustomMudFilter<T>
{
    public IEnumerable<EquatableCsvMappingError> SelectedItems { get; set; }
    public HashSet<EquatableCsvMappingError> FilterItems { get; set; }
    public List<EquatableCsvMappingError> AvailableItems { get; set; }
    
    public FilterDefinition<T> FilterDefinition { get; }

    public CsvMappingErrorMudFilter(FilterDefinition<T> filterDefinition)
    {
        SelectedItems = new HashSet<EquatableCsvMappingError>();
        FilterItems = new();
        AvailableItems = new();
        FilterDefinition = filterDefinition;
    }
    
    public async Task ApplyFilterOnContextAsync(FilterContext<T> filterContext)
    {
        FilterItems = SelectedItems.ToHashSet();
        await filterContext.Actions.ApplyFilterAsync(FilterDefinition);
    }

    public async Task ResetFilterOnContextAsync(FilterContext<T> filterContext)
    {
        ResetFilter();
        await filterContext.Actions.ClearFilterAsync(FilterDefinition);
    }

    public void ResetFilter()
    {
        SelectedItems = AvailableItems.ToHashSet();
        FilterItems = AvailableItems.ToHashSet();
    }
}