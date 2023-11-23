using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Rules : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private RulesPageViewModel _dataContext = null!;
    private bool _massEditEnabled;
    private bool _newMappingRuleSetIsEnabled;

    private BucketListingViewModel? _bucketSelectDialogDataContext;
    private bool _isBucketSelectDialogVisible;
    private bool _isBucketSelectDialogLoading;
    private RuleSetViewModel? _ruleSetViewModelToBeUpdated;

    private bool _isDeleteRuleSetDialogVisible;
    private RuleSetViewModel? _ruleSetToBeDeleted;

    private bool _isErrorModalDialogVisible;
    private string _errorModalDialogMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new RulesPageViewModel(ServiceManager);

        await HandleResult(await _dataContext.LoadDataAsync());
    }

    private void StartCreateMappingRuleSet()
    {
        _newMappingRuleSetIsEnabled = true;
    }

    private void CancelNewBucketRule()
    {
        _newMappingRuleSetIsEnabled = false;
        _dataContext.ResetNewRuleSet();
    }

    private void EditAllRules()
    {
        _massEditEnabled = true;
        _dataContext.EditAllRules();
    }

    private async void SaveAllRules()
    {
        _massEditEnabled = false;
        await HandleResult(_dataContext.SaveAllRules());
    }

    private async void CancelAllRules()
    {
        _massEditEnabled = false;
        await HandleResult(await _dataContext.LoadDataAsync());
        StateHasChanged();
    }

    private async void HandleShowSelectBucketDialog(RuleSetViewModel ruleSetViewModel)
    {
        _isBucketSelectDialogVisible = true;
        _isBucketSelectDialogLoading = true;
        
        _ruleSetViewModelToBeUpdated = ruleSetViewModel;
        _bucketSelectDialogDataContext = new BucketListingViewModel(ServiceManager, null);
        await _bucketSelectDialogDataContext.LoadDataAsync(false, true);
        
        _isBucketSelectDialogLoading = false;
        StateHasChanged();
    }

    private void UpdateSelectedBucket(BucketViewModel selectedBucket)
    {
        _ruleSetViewModelToBeUpdated!.UpdateSelectedBucket(selectedBucket);
        _isBucketSelectDialogVisible = false;
    }
    
    private void HandleShowDeleteRuleSetDialog(RuleSetViewModel ruleSet)
    {
        _ruleSetToBeDeleted = ruleSet;
        _isDeleteRuleSetDialogVisible = true;
    }

    private void CancelDeleteRule()
    {
        _isDeleteRuleSetDialogVisible = false;
        _ruleSetToBeDeleted = null;
    }

    private async void DeleteRule()
    {
        _isDeleteRuleSetDialogVisible = false;
        await HandleResult(_dataContext.DeleteRuleSetItem(_ruleSetToBeDeleted!));
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
}