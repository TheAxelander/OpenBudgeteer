using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.Common.CustomMudFilter;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.Helper;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Transaction : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;
    [Inject] private YearMonthSelectorViewModel YearMonthDataContext { get; set; } = null!;

    private TransactionPageViewModel _dataContext = null!;
    private TransactionViewModel _createTransactionDataContext = null!;
    private Tuple<TransactionViewModel, TransactionViewModel> _createTransferDataContext = null!;
    private bool _isEditModeEnabled;
    
    private DateOnlyMudFilter<TransactionViewModel> _dateOnlyMudFilter = null!;
    private EntityViewModelMudFilter<AccountViewModel, TransactionViewModel> _accountMudFilter = null!;
    private EntityViewModelMudFilter<PartialBucketViewModel, TransactionViewModel> _bucketMudFilter = null!;
    
    private HashSet<TransactionViewModel> _selectedTransactions = new();

    private RecurringTransactionHandlerViewModel? _recurringTransactionHandlerViewModel;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new TransactionPageViewModel(ServiceManager, YearMonthDataContext);
        _dateOnlyMudFilter = new(new()
        {
            FilterFunction = x => 
                x.TransactionDate.IsBetween(_dateOnlyMudFilter.DateRange.Start, _dateOnlyMudFilter.DateRange.End)
        });
        _accountMudFilter = new(new()
        {
            FilterFunction = x => _accountMudFilter.FilterItems.Contains(x.SelectedAccount)
        });
        _bucketMudFilter = new(new()
        {
            FilterFunction = x => x.Buckets.Any(bucket => _bucketMudFilter.FilterItems.Contains(bucket))
        });

        await ReloadDataContext();

        YearMonthDataContext.SelectedYearMonthChanged += async (sender, args) =>
        {
            await ReloadDataContext();
            StateHasChanged();
        };
    }

    private async Task ReloadDataContext()
    {
        await HandleResult(await _dataContext.LoadDataAsync());
        _createTransactionDataContext = TransactionViewModel.CreateEmpty(ServiceManager);
        _createTransferDataContext = new(
            TransactionViewModel.CreateEmpty(ServiceManager),
            TransactionViewModel.CreateEmpty(ServiceManager));
        _selectedTransactions.Clear();
        
        _accountMudFilter.AvailableItems = _dataContext.Transactions
            .Select(i => i.SelectedAccount)
            .Distinct()
            .OrderBy(i => i.Name)
            .ToList();
        _bucketMudFilter.AvailableItems = _dataContext.Transactions
            .SelectMany(i => i.Buckets)
            .DistinctBy(i => i.SelectedBucketId)
            .OrderBy(i => i.SelectedBucketName)
            .ToList();
        _bucketMudFilter.AvailableItems.Insert(0, PartialBucketViewModel.CreateNoSelection(ServiceManager));
        
        
        _accountMudFilter.ResetFilter();
        _dateOnlyMudFilter.ResetFilter();
        _bucketMudFilter.ResetFilter();
    }
    
    private void TransactionDateChanged(DateTime? dateTime, TransactionViewModel context)
    {
        context.TransactionDate = DateOnly.FromDateTime(dateTime ?? DateTime.Today);
    }
    
    private async Task ShowCreateTransactionDialog()
    {
        var reloadRequired = false;
        var lastEnteredDate = YearMonthDataContext.IsTodayInCurrentMonth ? 
            DateOnly.FromDateTime(DateTime.Today) : YearMonthDataContext.CurrentMonth;
        
        while (true)
        {
            _createTransactionDataContext.TransactionDate = lastEnteredDate;
            var createDialogParameters = new DialogParameters<CreateTransactionDialog>
            {
                { x => x.DataContext, _createTransactionDataContext }
            };
            var createDialog = await DialogService.ShowAsync<CreateTransactionDialog>(
                "Create Transactions", createDialogParameters);
            var createDialogResult = await createDialog.Result;
            if (createDialogResult is { Canceled: false })
            {
                var createItemResult = _dataContext.CreateItem(_createTransactionDataContext);
                if (createItemResult.IsSuccessful)
                {
                    reloadRequired = true;
                    if (createDialogResult.Data is CreateDialogResponse.CreateAnother)
                    {
                        lastEnteredDate = _createTransactionDataContext.TransactionDate;
                        continue;
                    }
                }
                else
                {
                    var errorDialogParameters = new DialogParameters<ErrorMessageDialog>
                    {
                        { x => x.Title, "Create Transaction" },
                        { x => x.Message, createItemResult.Message }
                    };
                    await DialogService.ShowAsync<ErrorMessageDialog>("Create Transaction", errorDialogParameters);
                }
            }

            break;
        }
        if (reloadRequired) await ReloadDataContext();
    }

    private async Task ShowCreateTransferDialog()
    {
        var reloadRequired = false;
        var lastEnteredDateSender = YearMonthDataContext.IsTodayInCurrentMonth ? 
            DateOnly.FromDateTime(DateTime.Today) : YearMonthDataContext.CurrentMonth;
        var lastEnteredDateReceiver = YearMonthDataContext.IsTodayInCurrentMonth ? 
            DateOnly.FromDateTime(DateTime.Today) : YearMonthDataContext.CurrentMonth;
        
        while (true)
        {
            var (sender, receiver) = _createTransferDataContext;
            sender.TransactionDate = lastEnteredDateSender;
            receiver.TransactionDate = lastEnteredDateReceiver;
            var createDialogParameters = new DialogParameters<CreateTransferDialog>
            {
                { x => x.SenderDataContext, sender },
                { x => x.ReceiverDataContext, receiver }
            };
            var createDialog = await DialogService.ShowAsync<CreateTransferDialog>(
                "Create Transfer", createDialogParameters);
            var createDialogResult = await createDialog.Result;
            if (createDialogResult is { Canceled: false })
            {
                sender.Buckets.Clear(); // Just to be sure
                receiver.Buckets.Clear(); // Just to be sure

                var transferBucket = new Core.Data.Entities.Models.Bucket()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    BucketGroupId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    BucketGroup = new BucketGroup
                    {
                        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                        Name = "System",
                        Position = 0
                    },
                    Name = "Transfer"
                };
                sender.Buckets.Add(PartialBucketViewModel.CreateFromBucket(ServiceManager, transferBucket, sender.Amount));
                receiver.Buckets.Add(PartialBucketViewModel.CreateFromBucket(ServiceManager, transferBucket, receiver.Amount));
                
                var createItemResultSender = sender.PerformConsistencyCheck();
                var createItemResultReceiver = receiver.PerformConsistencyCheck();

                if (createItemResultSender.IsSuccessful && createItemResultReceiver.IsSuccessful)
                {
                    createItemResultSender = _dataContext.CreateItem(sender);
                    createItemResultReceiver = _dataContext.CreateItem(receiver);
                }
                
                if (createItemResultSender.IsSuccessful && createItemResultReceiver.IsSuccessful)
                {
                    reloadRequired = true;
                    if (createDialogResult.Data is CreateDialogResponse.CreateAnother)
                    {
                        lastEnteredDateSender = sender.TransactionDate;
                        lastEnteredDateReceiver = receiver.TransactionDate;
                        continue;
                    }
                }
                else
                {
                    var messageStringBuilder = new StringBuilder();
                    if (!createItemResultSender.IsSuccessful)
                    {
                        messageStringBuilder.AppendLine("Sending Transaction:");
                        messageStringBuilder.AppendLine(createItemResultSender.Message);
                    }
                    if (!createItemResultReceiver.IsSuccessful)
                    {
                        if (messageStringBuilder.Length > 0) messageStringBuilder.AppendLine();
                        messageStringBuilder.AppendLine("Receiving Transaction:");
                        messageStringBuilder.AppendLine(createItemResultReceiver.Message);
                    }
                    
                    var errorDialogParameters = new DialogParameters<ErrorMessageDialog>
                    {
                        { x => x.Title, "Create Transfer" },
                        { x => x.Message, messageStringBuilder.ToString() }
                    };
                    await DialogService.ShowAsync<ErrorMessageDialog>("Create Transfer", errorDialogParameters);
                }
            }

            break;
        }
        if (reloadRequired) await ReloadDataContext();
    }

    private void SwitchToEditMode()
    {
        _isEditModeEnabled = true;
        var transactionsToModify = _selectedTransactions.Count > 0 
            ? _selectedTransactions.ToList() 
            : _dataContext.Transactions.ToList();
        foreach (var transaction in transactionsToModify)
        {
            transaction.StartModification();
        }
    }

    private async Task DeleteSelectedTransactions()
    {
        var parameters = new DialogParameters<DeleteConfirmationDialog>
        {
            { x => x.Title, "Delete Transactions" },
            { x => x.Message, "Do you really want to delete the selected Transactions?" }
        };
        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>("Delete Transactions", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            var deletionResults = _selectedTransactions
                .Select(i => i.DeleteTransaction())
                .ToList();
            if (deletionResults.Any(i => !i.IsSuccessful))
            {
                await HandleResult(deletionResults.First(i => !i.IsSuccessful));
            }
            else
            {
                await HandleResult(deletionResults.First());
            }
            _selectedTransactions.Clear();
        }
    }

    private async Task ProposeBucketsAsync()
    {
        var parameters = new DialogParameters<InfoDialog>
        {
            { x => x.Title, "Propose Buckets" },
            { x => x.Message, "Searching Buckets based on defined rules..." },
            { x => x.IsInteractionEnabled, false }
        };
        var dialog = await DialogService.ShowAsync<InfoDialog>("Propose Buckets", parameters);

        await _dataContext.ProposeBuckets();
        _isEditModeEnabled = true;
        dialog.Close();
    }
    
    private void Transactions_SelectionChanged(HashSet<TransactionViewModel> items)
    {
        _selectedTransactions = items;
    }

    private async Task SaveAllTransaction()
    {
        _isEditModeEnabled = false;
        await HandleResult(_dataContext.SaveAllTransaction());
    }

    private async Task CancelAllTransaction()
    {
        _isEditModeEnabled = false;
        await ReloadDataContext();
        StateHasChanged();
    }

    private async Task AddRecurringTransactions()
    {
        await HandleResult(await _dataContext.AddRecurringTransactionsAsync());
    }
    
    private bool Transactions_QuickFilter(TransactionViewModel transactionViewModel)
    {
        if (!_isEditModeEnabled) return true; // Display all items if Edit Mode is not enabled 
        
        return transactionViewModel.InModification;
    }

    private async Task ShowRecurringTransactionDialog()
    {
        _recurringTransactionHandlerViewModel = new RecurringTransactionHandlerViewModel(ServiceManager);
        await _recurringTransactionHandlerViewModel.LoadDataAsync();
        var parameters = new DialogParameters<RecurringTransactionDialog>
        {
            { x => x.DataContext, _recurringTransactionHandlerViewModel }
        };
        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.ExtraLarge,
            FullWidth = true
        };
        await DialogService.ShowAsync<RecurringTransactionDialog>("Recurring Transactions", parameters, options);
    }

    private async Task ShowBucketSelectDialog(TransactionViewModel transactionViewModel, PartialBucketViewModel partialBucketViewModel)
    {
        var bucketSelectDialogDataContext = new BucketListingViewModel(ServiceManager, YearMonthDataContext);
        await HandleResult(await bucketSelectDialogDataContext.LoadDataForSelectionScreenAsync());
        
        var parameters = new DialogParameters<BucketSelectDialog>
        {
            { x => x.DataContext, bucketSelectDialogDataContext }
        };
        var options = new DialogOptions()
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true
        };
        var dialog = await DialogService.ShowAsync<BucketSelectDialog>("Select Bucket", parameters, options);
        var dialogResult = await dialog.Result;
        if (dialogResult is { Canceled: false, Data: BucketViewModel selectedBucket })
        {
            partialBucketViewModel.UpdateSelectedBucket(selectedBucket);
            if (partialBucketViewModel.Amount == 0)
            {
                partialBucketViewModel.Amount = 
                    transactionViewModel.Amount - 
                    transactionViewModel.Buckets
                        .Where(i => i.SelectedBucketId != partialBucketViewModel.SelectedBucketId)
                        .Sum(i => i.Amount);
            }
        }
      
        StateHasChanged();
    }

    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Transaction" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Transaction", parameters);
        }
		if (result.ViewModelReloadRequired)
        {
            await ReloadDataContext();
            StateHasChanged();
        }
    }
}