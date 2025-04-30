using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Index : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private AppSettingService AppSettingService { get; set; } = null!;
    
    private HomePageViewModel _dataContext = null!;
    
    protected override async Task OnInitializedAsync()
    {
        _dataContext = new HomePageViewModel(ServiceManager);
        await HandleResult(_dataContext.LoadData(AppSettingService.CurrentSettings.HomePageTopCount));
    }
    
    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Home" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Home", parameters);
        }
        if (result.ViewModelReloadRequired)
        {
            _dataContext.LoadData(AppSettingService.CurrentSettings.HomePageTopCount);
            StateHasChanged();
        }
    }
}