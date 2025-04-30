using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Bucket : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;
    
    private MudTable<BucketViewModel>? _bucketTableRef;

    private BucketPageViewModel _dataContext = null!;

    private readonly TableGroupDefinition<BucketViewModel> _groupByBucketGroup = new()
    {
        GroupName = "Bucket Group:",
        Expandable = true,
        Selector = x => x.BucketGroupViewModel!
    };
    
    protected override async Task OnInitializedAsync()
    {
        _dataContext = new BucketPageViewModel(ServiceManager, YearMonthDataContext);

        await HandleResult(await _dataContext.LoadDataAsync());
        
        YearMonthDataContext.SelectedYearMonthChanged += async (sender, args) => 
        {
            await HandleResult(await _dataContext.LoadDataAsync());
            StateHasChanged();
        };
    }

    private async Task DistributeBudget()
    {
        await HandleResult(_dataContext.DistributeBudget());
    }

    private async Task ShowBucketGroupDialog()
    {
        var parameters = new DialogParameters<BucketGroupDialog>
        {
            { x => x.DataContext, _dataContext.BucketGroups }
        };
        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var dialog = await DialogService.ShowAsync<BucketGroupDialog>("Manage Bucket Group", parameters, options);
        await dialog.Result;
        await _dataContext.LoadDataAsync();
        StateHasChanged();
    }

    private async Task ShowEditBucketDialog()
    {
        var newBucket = BucketViewModel.CreateEmpty(ServiceManager, _dataContext.BucketGroups.First().BucketGroupId, YearMonthDataContext.CurrentMonth); 
        await ShowEditBucketDialog(newBucket);
    }

    private async Task ShowEditBucketDialog(BucketViewModel bucket)
    {
        var parameters = new DialogParameters<EditBucketDialog>
        {
            { x => x.Title, "Edit Bucket" },
            { x => x.DataContext, bucket }
        };
        var dialog = await DialogService.ShowAsync<EditBucketDialog>("Edit Bucket", parameters);
        var dialogResult = await dialog.Result;
        if (dialogResult is { Canceled: false })
        {
            var result = _dataContext.SaveChanges(bucket);
            await HandleResult(result);
        }
        else
        {
            await HandleResult(await _dataContext.LoadDataAsync());
        }
        StateHasChanged();
    }

    private async Task CloseBucket(BucketViewModel bucket)
    {
        var parameters = new DialogParameters<DeleteConfirmationDialog>
        {
            { x => x.Title, "Close Bucket" },
            { x => x.Message, "Do you really want to close this Bucket?" }
        };
        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>("Close Bucket", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await HandleResult(_dataContext.CloseBucket(bucket));
            StateHasChanged();
        }
    }

    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Bucket" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Bucket", parameters);
        }
		if (result.ViewModelReloadRequired)
        {
            await _dataContext.LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task InOut_Changed(BucketViewModel bucket, KeyboardEventArgs args)
    {
        if (args.Key != "Enter") return;
        var result = bucket.HandleInOutInput();
        if (result.IsSuccessful)
        {
            await HandleResult(_dataContext.UpdateBalanceFigures());
            StateHasChanged();
        }
        else
        {
            await HandleResult(result);
        }
    }

    private async Task DisplayBucketDetails(BucketViewModel bucket)
    {
        var includeBucketMovements = true;
        var dialogDataContext =
            new BlazorBucketStatisticsViewModel(ServiceManager, YearMonthDataContext, bucket.BucketId);
        await dialogDataContext.LoadDataAsync(includeBucketMovements);
        var parameters = new DialogParameters<BucketDetailsDialog>
        {
            { x => x.Title, "Bucket Details" },
            { x => x.DataContext, dialogDataContext },
            { x => x.IncludeBucketMovements, includeBucketMovements }
        };
        var dialogOptions = new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Large
        };
        await DialogService.ShowAsync<BucketDetailsDialog>("Bucket Details", parameters, dialogOptions);
    }

    // Determines the color of the balance based on its value
    private string GetBalanceColor(decimal balance)
    {
        if (balance == 0)
            return string.Empty;

        return balance < 0 ? "color: red;" : "color: green;";
    }
}