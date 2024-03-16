using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenBudgeteer.Blazor.ViewModels;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Bucket : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;

    private BucketPageViewModel _dataContext = null!;

    private bool _isNewBucketGroupModalDialogVisible;

    private BucketViewModel? _editBucketDialogDataContext;
    private bool _isEditBucketModalDialogVisible;

    private BlazorBucketStatisticsViewModel? _bucketDetailsModalDialogDataContext;
    private bool _isBucketDetailsModalDialogVisible;
    private bool _isBucketDetailsModalDialogDataContextLoading;

    private bool _isDeleteBucketGroupDialogVisible;
    private BucketGroupViewModel? _bucketGroupToBeDeleted;
    
    private bool _isCloseBucketDialogVisible;
    private BucketViewModel? _bucketToBeClosed;

    private bool _isErrorModalDialogVisible;
    private string _errorModalDialogMessage = string.Empty;
    private bool _hasErrorInBucketModalDialog;

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

    private async void DistributeBudget()
    {
        await HandleResult(_dataContext.DistributeBudget());
    }

    private void CreateBucket(BucketGroupViewModel bucketGroup)
    {
        var newBucket = bucketGroup.CreateBucket();
        ShowEditBucketDialog(newBucket);
    }

    private void ShowNewBucketGroupDialog()
    {
        _dataContext.CreateEmptyGroup();
        _isNewBucketGroupModalDialogVisible = true;
    }

    private async void SaveAndCloseNewBucketGroupDialog()
    {
        _isNewBucketGroupModalDialogVisible = false;
        await HandleResult(_dataContext.NewBucketGroup!.CreateGroup());
    }

    private void CancelNewBucketGroupDialog()
    {
        _isNewBucketGroupModalDialogVisible = false;
    }
    
    private void HandleBucketGroupDeleteRequest(BucketGroupViewModel bucketGroup)
    {
        _bucketGroupToBeDeleted = bucketGroup;
        _isDeleteBucketGroupDialogVisible = true;
    }
    
    private void CancelDeleteBucketGroup()
    {
        _isDeleteBucketGroupDialogVisible = false;
        _bucketGroupToBeDeleted = null;
    }
    
    private async void DeleteBucketGroup()
    {
        _isDeleteBucketGroupDialogVisible = false;
        if(_bucketGroupToBeDeleted != null) await HandleResult(_bucketGroupToBeDeleted.DeleteGroup());
        _bucketGroupToBeDeleted = null;
        StateHasChanged();
    }

    private void ShowEditBucketDialog(BucketViewModel bucket)
    {
        _editBucketDialogDataContext = bucket;
        _isEditBucketModalDialogVisible = true;
    }

    private async void SaveAndCloseEditBucketDialog()
    {
        _isEditBucketModalDialogVisible = false;
        var result = _dataContext.SaveChanges(_editBucketDialogDataContext!);
        await HandleResult(result);
        if (!result.IsSuccessful)
        {
            _hasErrorInBucketModalDialog = true; // Ensures that Dialog will be displayed again
            return; // Error message is shown in HandleResult()
        }
        StateHasChanged();
    }

    private async void CancelEditBucketDialog()
    {
        _isEditBucketModalDialogVisible = false;
        await HandleResult(await _dataContext.LoadDataAsync());
        StateHasChanged();
    }

    private void HandleBucketCloseRequest(BucketViewModel bucket)
    {
        _bucketToBeClosed = bucket;
        _isCloseBucketDialogVisible = true;
    }

    private void CancelCloseBucket()
    {
        _isCloseBucketDialogVisible = false;
        _bucketToBeClosed = null;
    }

    private async void CloseBucket()
    {
        _isCloseBucketDialogVisible = false;
        await HandleResult(_dataContext.CloseBucket(_bucketToBeClosed!));
        StateHasChanged();
    }

    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            _errorModalDialogMessage = result.Message;
            _isErrorModalDialogVisible = true;
        }
		if (result.ViewModelReloadRequired)
        {
            await _dataContext.LoadDataAsync();
            StateHasChanged();
        }
    }

    private async void InOut_Changed(BucketViewModel bucket, KeyboardEventArgs args)
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

    private async void DisplayBucketDetails(BucketViewModel bucket)
    {
        _isBucketDetailsModalDialogVisible = true;
        _isBucketDetailsModalDialogDataContextLoading = true;

        _bucketDetailsModalDialogDataContext = new BlazorBucketStatisticsViewModel(ServiceManager, YearMonthDataContext, bucket.BucketId);
        await _bucketDetailsModalDialogDataContext.LoadDataAsync(true);

        _isBucketDetailsModalDialogDataContextLoading = false;
        StateHasChanged();
    }

    private void CloseErrorDialog()
    {
        _isErrorModalDialogVisible = false;
        // In case error occuring in EditBucketDialog, display it again
        if (_hasErrorInBucketModalDialog)
        {
            _isEditBucketModalDialogVisible = true;
            _hasErrorInBucketModalDialog = false;
        }
    }
}