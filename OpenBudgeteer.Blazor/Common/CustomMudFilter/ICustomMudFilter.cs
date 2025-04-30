using System.Threading.Tasks;
using MudBlazor;

namespace OpenBudgeteer.Blazor.Common.CustomMudFilter;

public interface ICustomMudFilter<T>
{
    public FilterDefinition<T> FilterDefinition { get; }
    
    public Task ApplyFilterOnContextAsync(FilterContext<T> filterContext);
    public Task ResetFilterOnContextAsync(FilterContext<T> filterContext);
    public void ResetFilter();
}