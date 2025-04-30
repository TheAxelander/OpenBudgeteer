using System.Threading.Tasks;
using MudBlazor;

namespace OpenBudgeteer.Blazor.Common.CustomMudFilter;

public class DateOnlyMudFilter<T> : ICustomMudFilter<T>
{
    public DateRange DateRange { get; set; }
    public FilterDefinition<T> FilterDefinition { get; }
    
    public DateOnlyMudFilter(FilterDefinition<T> filterDefinition)
    {
        DateRange = new DateRange();
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
        DateRange = new();
    }
}