using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Photino.Pages;

public partial class Account : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private AccountPageViewModel _dataContext = null!;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new AccountPageViewModel(ServiceManager);
        await HandleResult(_dataContext.LoadData());
    }

    private async Task ShowEditAccountDialog(AccountViewModel? account)
    {
        var dialogTitle = "Edit Account";
        if (account is null)
        {
            account = AccountViewModel.CreateEmpty(ServiceManager);
            dialogTitle = "New Account";
        }
        
        var parameters = new DialogParameters<EditAccountDialog>
        {
            { x => x.Title, dialogTitle },
            { x => x.DataContext, account }
        };
        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
        var dialog = await DialogService.ShowAsync<EditAccountDialog>(dialogTitle, parameters, options);
        var dialogResult = await dialog.Result;
        if (dialogResult is { Canceled: false })
        {
            await HandleResult(account.CreateOrUpdateAccount());
        }
        else
        {
            await HandleResult(_dataContext.LoadData());
        }
    }

    private async Task ShowCloseAccountDialog(AccountViewModel account)
    {
        var parameters = new DialogParameters<DeleteConfirmationDialog>
        {
            { x => x.Title, "Close Account" },
            { x => x.Message, "Do you really want to close this Account? Ensure that the balance is 0." }
        };
        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>("Close Account", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await HandleResult(account.CloseAccount());
        }
    }
    
    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Account" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Account", parameters);
        }
        if (result.ViewModelReloadRequired)
        {
            _dataContext.LoadData();
            StateHasChanged();
        }
    }

    private async Task ShowTransactionListingDialog(AccountViewModel account)
    {
        var transactionDialogDataContext = new TransactionListingViewModel(ServiceManager);
        await HandleResult(await transactionDialogDataContext.LoadDataAsync(account.AccountId));
        var parameters = new DialogParameters<TransactionDialog>
        {
            { x => x.Title, "Account Transactions" },
            { x => x.DataContext, transactionDialogDataContext }
        };
        var options = new DialogOptions()
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Large
        };
        await DialogService.ShowAsync<TransactionDialog>("Account", parameters, options);
    }
}