using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Account : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private AccountPageViewModel _dataContext = null!;
    private TransactionListingViewModel? _transactionModalDialogDataContext;

    private bool _isEditAccountModalDialogVisible;
    private string _editAccountDialogTitle = string.Empty;
    private AccountViewModel? _editAccountDialogDataContext;

    private bool _isTransactionModalDialogVisible;
    private bool _isTransactionModalDialogDataContextLoading;

    private bool _isErrorModalDialogVisible;
    private string _errorModalDialogMessage = string.Empty;

    protected override void OnInitialized()
    {
        _dataContext = new AccountPageViewModel(ServiceManager);
        HandleResult(_dataContext.LoadData());
    }

    private void CreateNewAccount()
    {
        _editAccountDialogTitle = "New Account";
        _editAccountDialogDataContext = AccountViewModel.CreateEmpty(ServiceManager);
        _isEditAccountModalDialogVisible = true;
    }

    private void EditAccount(AccountViewModel account)
    {
        _editAccountDialogTitle = "Edit Account";
        _editAccountDialogDataContext = account;
        _isEditAccountModalDialogVisible = true;
    }


    private void SaveChanges(AccountViewModel account)
    {
        _isEditAccountModalDialogVisible = false;
        HandleResult(account.CreateOrUpdateAccount());
    }

    private void CancelChanges()
    {
        _isEditAccountModalDialogVisible = false;
        HandleResult(_dataContext.LoadData());
    }

    private void CloseAccount(AccountViewModel account)
    {
        HandleResult(account.CloseAccount());
    }

    private void HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            _errorModalDialogMessage = result.Message;
            _isErrorModalDialogVisible = true;
        }
        if (result.ViewModelReloadRequired)
        {
            _dataContext.LoadData();
            StateHasChanged();
        }
    }

    private async void DisplayAccountTransactions(AccountViewModel account)
    {
        _isTransactionModalDialogVisible = true;
        _isTransactionModalDialogDataContextLoading = true;

        _transactionModalDialogDataContext = new TransactionListingViewModel(ServiceManager);
        HandleResult(await _transactionModalDialogDataContext.LoadDataAsync(account.AccountId));

        _isTransactionModalDialogDataContextLoading = false;
        StateHasChanged();
    }
}