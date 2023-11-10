using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class DataConsistency : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    
    DataConsistencyPageViewModel _dataContext = null!;

    private bool _isLoadingDialogVisible;

    protected override async Task OnInitializedAsync()
    {
        _isLoadingDialogVisible = true;
        StateHasChanged();
        _dataContext = new DataConsistencyPageViewModel(ServiceManager);
        await _dataContext.RunAllChecksAsync();
        _isLoadingDialogVisible = false;
    }
}