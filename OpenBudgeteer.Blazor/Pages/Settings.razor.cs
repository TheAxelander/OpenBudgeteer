using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Settings : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private MudThemeService MudThemeService { get; set; } = null!;
    
    private SettingsPageViewModel _dataContext = null!;
    private bool _showAll;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new SettingsPageViewModel(ServiceManager, MudThemeService);
        await RestoreThemeAsync();
    }
    
    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Settings" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Settings", parameters);
        }
        if (result.ViewModelReloadRequired)
        {
            await _dataContext.LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task ApplyThemeAsync()
    {
        await HandleResult(await _dataContext.ApplyThemeAsync());
        StateHasChanged();
    }

    private async Task RestoreThemeAsync()
    {
        await HandleResult(await _dataContext.LoadDataAsync());
        StateHasChanged();
    }
    
    private async Task RestoreDefaultThemeAsync()
    {
        await _dataContext.RestoreDefaultThemeAsync();
        StateHasChanged();
    }
}