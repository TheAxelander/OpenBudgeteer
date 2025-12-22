using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.AppSettings;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Photino.ViewModels;

namespace OpenBudgeteer.Photino.Pages;

public partial class Settings : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private IMudThemeService MudThemeService { get; set; } = null!;
    [Inject] private IAppSettingService AppSettingService { get; set; } = null!;

    private PhotinoSettingsPageViewModel _dataContext = null!;
    private bool _showAll;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new PhotinoSettingsPageViewModel(ServiceManager, MudThemeService, AppSettingService);
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

    private async Task ApplySettingsAsync()
    {
        await HandleResult(await _dataContext.ApplySettingsAsync());
        StateHasChanged();
    }

    private async Task RestoreSettingsAsync()
    {
        await HandleResult(await _dataContext.LoadDataAsync());
        StateHasChanged();
    }
}
