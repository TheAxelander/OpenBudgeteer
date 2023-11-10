using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Transaction : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;

    private TransactionPageViewModel _dataContext = null!;
    private bool _newTransactionEnabled;
    private bool _massEditEnabled;

    private BucketListingViewModel? _bucketSelectDialogDataContext;
    private bool _isBucketSelectDialogVisible;
    private bool _isBucketSelectDialogLoading;
    private PartialBucketViewModel? _partialBucketViewModelToBeUpdated;

    private bool _isRecurringTransactionModalDialogVisible;
    private RecurringTransactionHandlerViewModel? _recurringTransactionHandlerViewModel;

    private bool _isDeleteTransactionDialogVisible;
    private TransactionViewModel? _transactionToBeDeleted;

    private bool _isErrorModalDialogVisible;
    private string _errorModalDialogMessage = string.Empty;

    private bool _isProposeBucketsInfoDialogVisible;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new TransactionPageViewModel(ServiceManager, YearMonthDataContext);

        await HandleResult(await _dataContext.LoadDataAsync());

        YearMonthDataContext.SelectedYearMonthChanged += async (sender, args) =>
        {
            await HandleResult(await _dataContext.LoadDataAsync());
            StateHasChanged();
        };
    }

    private void StartCreateNewTransaction()
    {
        _newTransactionEnabled = true;
    }

    private void CancelNewTransaction()
    {
        _newTransactionEnabled = false;
        _dataContext.ResetNewTransaction();
    }

    private void EditAllTransaction()
    {
        _massEditEnabled = true;
        _dataContext.EditAllTransaction();
    }

    private void NewTransactionAccount_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _dataContext.NewTransaction!.Transaction.Account = 
            _dataContext.NewTransaction.AvailableAccounts.First(i => i.Id == Guid.Parse(value));
    }

    private async Task ProposeBucketsAsync()
    {
        _isProposeBucketsInfoDialogVisible = true;
        StateHasChanged();
        await _dataContext.ProposeBuckets();
        if (_dataContext.Transactions.Any(i => i.InModification)) _massEditEnabled = true;
        _isProposeBucketsInfoDialogVisible = false;
    }

    private void TransactionAccount_SelectionChanged(string? value, TransactionViewModel transactionViewModel)
    {
        if (string.IsNullOrEmpty(value)) return;
        transactionViewModel.Transaction.Account = 
            transactionViewModel.AvailableAccounts.First(i => i.Id == Guid.Parse(value));
    }

    private void SplitTransaction(TransactionViewModel transaction) =>
        transaction.AddBucketItem(transaction.Transaction.Amount - transaction.Buckets.Sum(b => b.Amount));

    private async void SaveAllTransaction()
    {
        _massEditEnabled = false;
        await HandleResult(_dataContext.SaveAllTransaction());
    }

    private async void CancelAllTransaction()
    {
        _massEditEnabled = false;
        await HandleResult(await _dataContext.CancelAllTransactionAsync());
        StateHasChanged();
    }

    private async void SaveTransaction(TransactionViewModel transaction)
    {
        await HandleResult(transaction.UpdateItem());
    }

    private void Filter_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        _dataContext.CurrentFilter = Enum.TryParse(typeof(TransactionFilter), value, out var result) 
            ? (TransactionFilter)result 
            : TransactionFilter.NoFilter;
    }

    private void HandleShowDeletionTransactionDialog(TransactionViewModel transaction)
    {
        _transactionToBeDeleted = transaction;
        _isDeleteTransactionDialogVisible = true;
    }

    private void CancelDeleteTransaction()
    {
        _isDeleteTransactionDialogVisible = false;
        _transactionToBeDeleted = null;
    }

    private async void DeleteTransaction()
    {
        _isDeleteTransactionDialogVisible = false;
        await HandleResult(_transactionToBeDeleted!.DeleteItem());
    }

    private async void AddRecurringTransactions()
    {
        await HandleResult(await _dataContext.AddRecurringTransactionsAsync());
    }

    private async Task DisplayRecurringTransactions()
    {
        _recurringTransactionHandlerViewModel = new RecurringTransactionHandlerViewModel(ServiceManager);
        await _recurringTransactionHandlerViewModel.LoadDataAsync();
        _isRecurringTransactionModalDialogVisible = true;
    }

    private async void HandleShowBucketSelectDialog(PartialBucketViewModel partialBucketViewModel)
    {
        _isBucketSelectDialogVisible = true;
        _isBucketSelectDialogLoading = true;
        
        _partialBucketViewModelToBeUpdated = partialBucketViewModel;
        _bucketSelectDialogDataContext = new BucketListingViewModel(ServiceManager, YearMonthDataContext);
        await _bucketSelectDialogDataContext.LoadDataAsync(true, true);
        
        _isBucketSelectDialogLoading = false;
        StateHasChanged();
    }

    private void UpdateSelectedBucket(Core.Data.Entities.Models.Bucket selectedBucket)
    {
        _partialBucketViewModelToBeUpdated!.SelectedBucket = selectedBucket;
        _isBucketSelectDialogVisible = false;
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