using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class DataConsistency : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    
    DataConsistencyPageViewModel _dataContext = null!;

    protected override void OnInitialized()
    {
        _dataContext = new DataConsistencyPageViewModel(ServiceManager);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Needs to run here, otherwise the Dialog is not shown
        if (!firstRender) return;
        var parameters = new DialogParameters<InfoDialog>
        {
            { x => x.Title, "Check Data Consistency" },
            { x => x.Message, "Execute several checks on your data..." },
            { x => x.IsInteractionEnabled, false }
        };
        var dialog = await DialogService.ShowAsync<InfoDialog>("Check Data Consistency", parameters);

        await _dataContext.RunAllChecksAsync();
        dialog.Close();
        StateHasChanged();
    }

    private Severity GetSeverity(DataConsistencyCheckResult.StatusCode checkResult)
    {
        switch (checkResult)
        {
            case DataConsistencyCheckResult.StatusCode.Ok:
                return Severity.Success;
            case DataConsistencyCheckResult.StatusCode.Warning:
                return Severity.Warning;
            case DataConsistencyCheckResult.StatusCode.Alert:
            default:
                return Severity.Error;
        }
    }
}