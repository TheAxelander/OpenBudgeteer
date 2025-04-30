using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;

namespace OpenBudgeteer.Blazor.Common.CustomMudFilter;

public class EntityViewModelMudFilter<TSource, TTarget> : ICustomMudFilter<TTarget>
{
    public IEnumerable<TSource> SelectedItems { get; set; }
    public HashSet<TSource> FilterItems { get; set; }
    public List<TSource> AvailableItems { get; set; }

    public FilterDefinition<TTarget> FilterDefinition { get; }

    public EntityViewModelMudFilter(FilterDefinition<TTarget> filterDefinition)
    {
        SelectedItems = new HashSet<TSource>();
        FilterItems = new();
        AvailableItems = new();
        FilterDefinition = filterDefinition;
    }

    public async Task ApplyFilterOnContextAsync(FilterContext<TTarget> filterContext)
    {
        FilterItems = SelectedItems.ToHashSet();
        await filterContext.Actions.ApplyFilterAsync(FilterDefinition);
    }

    public async Task ResetFilterOnContextAsync(FilterContext<TTarget> filterContext)
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