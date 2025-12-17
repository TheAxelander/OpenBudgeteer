using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OpenBudgeteer.Blazor.Common;
using OpenBudgeteer.Blazor.Common.CustomMudFilter;
using OpenBudgeteer.Blazor.Common.InputLargeTextArea;
using OpenBudgeteer.Blazor.Shared.Dialog;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Common.Extensions;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.PageViewModels;
using TinyCsvParser.Mapping;

namespace OpenBudgeteer.Photino.Pages;

public partial class Import : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private ImportPageViewModel _dataContext = null!;

    private MudStepper? _stepper;
    private MudFileUpload<IBrowserFile>? _fileUpload;
    private string? _selectedFileName;
    private InputLargeTextArea? _previewTextArea;
    private CancellationTokenSource? _previewOnChangeCancellationTokenSource;

    private string DelimiterHelper
    {
        get => _dataContext.ModifiedImportProfile.Delimiter == '\0' ?
            string.Empty : _dataContext.ModifiedImportProfile.Delimiter.ToString();
        set
        {
            if (value.Length > 1) return;
            _dataContext.ModifiedImportProfile.Delimiter = Convert.ToChar(value);
        }
    }

    private string TextQualifierHelper
    {
        get => _dataContext.ModifiedImportProfile.TextQualifier == '\0' ?
            string.Empty : _dataContext.ModifiedImportProfile.TextQualifier.ToString();
        set
        {
            if (value.Length > 1) return;
            _dataContext.ModifiedImportProfile.TextQualifier = Convert.ToChar(value);
        }
    }

    private enum MaxStepLevel { Step1, Step2, Step3, Step4 }
    private MaxStepLevel _maxStepLevel = MaxStepLevel.Step1;
    private int _stepperIndex;

    private DateOnlyMudFilter<CsvMappingResult<ParsedBankTransaction>> _validRecordsDateOnlyMudFilter = null!;
    private DateOnlyMudFilter<ImportPageViewModel.Duplicate> _duplicateRecordsDateOnlyMudFilter = null!;
    private CsvMappingErrorMudFilter<CsvMappingResult<ParsedBankTransaction>> _errorRecordsMudFilter = null!;

    private bool _isValidationRunning;
    private bool _isImportRunning;

    private string _validationErrorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _dataContext = new ImportPageViewModel(ServiceManager);
        _validRecordsDateOnlyMudFilter = new(new()
        {
            FilterFunction = x =>
                DateOnly.FromDateTime(x.Result.TransactionDate).IsBetween(
                    _validRecordsDateOnlyMudFilter.DateRange.Start,
                    _validRecordsDateOnlyMudFilter.DateRange.End)
        });
        _duplicateRecordsDateOnlyMudFilter = new(new()
        {
            FilterFunction = x =>
                DateOnly.FromDateTime(x.ParsedBankTransaction.Result.TransactionDate).IsBetween(
                    _duplicateRecordsDateOnlyMudFilter.DateRange.Start,
                    _duplicateRecordsDateOnlyMudFilter.DateRange.End)
        });
        _errorRecordsMudFilter = new(new()
        {
            FilterFunction = x =>
                _errorRecordsMudFilter.FilterItems.Contains(new EquatableCsvMappingError(x.Error))
        });

        await LoadData();
    }

    private async Task LoadData()
    {
        await HandleResult(_dataContext.LoadData());
        _maxStepLevel = MaxStepLevel.Step1;
    }

    private Task OnPreviewInteraction(StepperInteractionEventArgs arg)
    {
        // occurs when clicking next or on a step header
        if (arg.Action is not (StepAction.Activate or StepAction.Complete)) return Task.CompletedTask;
        switch (arg.StepIndex)
        {
            case 1:
                if (_maxStepLevel == MaxStepLevel.Step1) arg.Cancel = true;
                break;
            case 2:
                if (_maxStepLevel is MaxStepLevel.Step1 or MaxStepLevel.Step2) arg.Cancel = true;
                break;
            case 3:
                if (_maxStepLevel is not MaxStepLevel.Step4) arg.Cancel = true;
                break;
        }

        return Task.CompletedTask;
    }

    private async Task UploadFile(IBrowserFile? file)
    {
        if (file is null)
        {
            _selectedFileName = null;
            return;
        }
        await LoadData();

        var parameters = new DialogParameters<InfoDialog>
        {
            { x => x.Title, "Import" },
            { x => x.Message, "Uploading and processing file..." },
            { x => x.IsInteractionEnabled, false }
        };
        var dialog = await DialogService.ShowAsync<InfoDialog>("Import", parameters);

        _selectedFileName = file.Name;
        await HandleResult(await _dataContext.HandleOpenFileAsync(file.OpenReadStream()));
        await SyncViewModelFileTextToPreviewTextAsync();

        dialog.Close();
        _maxStepLevel = MaxStepLevel.Step2;
    }

    private async Task SyncPreviewTextToViewModelFileTextAsync()
    {
        var streamReader = await _previewTextArea!.GetTextAsync(maxLength: 100_000);
        var textFromInputLargeTextArea = await streamReader.ReadToEndAsync();
        _dataContext.FileText = textFromInputLargeTextArea;
    }

    private async Task SyncViewModelFileTextToPreviewTextAsync()
    {
        var textToWrite = _dataContext.FileText;

        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        await streamWriter.WriteAsync(textToWrite);
        await streamWriter.FlushAsync();
        await _previewTextArea!.SetTextAsync(streamWriter);
    }

    private async Task PreviewTextAreaChangedAsync(InputLargeTextAreaChangeEventArgs args)
    {
        // Cancel the previous task if it exists
        _previewOnChangeCancellationTokenSource?.Cancel();
        _previewOnChangeCancellationTokenSource = new();

        await Task.Run(async () => await SyncPreviewTextToViewModelFileTextAsync(),
            _previewOnChangeCancellationTokenSource.Token);
    }

    private void SelectedImportProfile_SelectionChanged()
    {
        _dataContext.ResetLoadFigures();

        if (_dataContext.SelectedImportProfile is not null && _dataContext.IdentifiedColumns.Any())
        {
            _maxStepLevel = MaxStepLevel.Step3;
            CheckColumnMapping(); // Checks on Step 4
        }

        StateHasChanged();
    }

    private async Task CreateProfile()
    {
        var result = _dataContext.CreateProfile();

        if (result.IsSuccessful)
        {
            var parameters = new DialogParameters<InfoDialog>
            {
                { x => x.Title, "Import" },
                { x => x.Message, "Profile has been created." },
                { x => x.IsInteractionEnabled, true }
            };
            await DialogService.ShowAsync<InfoDialog>("Import", parameters);
        }
        else
        {
            await HandleResult(result);
        }
    }

    private async Task SaveProfile()
    {
        var result = _dataContext.SaveProfile();

        if (result.IsSuccessful)
        {
            var parameters = new DialogParameters<InfoDialog>
            {
                { x => x.Title, "Import" },
                { x => x.Message, "Changes for Profile have been saved." },
                { x => x.IsInteractionEnabled, true }
            };
            await DialogService.ShowAsync<InfoDialog>("Import", parameters);
        }
        else
        {
            await HandleResult(result);
        }
    }

    private async Task DeleteProfile()
    {
        var parameters = new DialogParameters<DeleteConfirmationDialog>
        {
            { x => x.Title, "Delete Import Profile" },
            { x => x.Message, "Do you really want to delete the selected Import Profile?" }
        };
        var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>("Delete Import Profile", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await HandleResult(_dataContext.DeleteProfile());
            await LoadData();
            _maxStepLevel = MaxStepLevel.Step2;
        }
    }

    private async Task LoadHeaders()
    {
        var result = _dataContext.LoadHeaders();
        if (result.IsSuccessful)
        {
            _maxStepLevel = MaxStepLevel.Step3;
            CheckColumnMapping();
        }
        else
        {
            _maxStepLevel = MaxStepLevel.Step2;
            await HandleResult(result);
        }
    }

    private void CheckColumnMapping()
    {
        _maxStepLevel = MaxStepLevel.Step3;
        // Check on mandatory column mapping
        if (string.IsNullOrEmpty(_dataContext.ModifiedImportProfile.TransactionDateColumnName)) return;
        if (string.IsNullOrEmpty(_dataContext.ModifiedImportProfile.MemoColumnName)) return;
        if (string.IsNullOrEmpty(_dataContext.ModifiedImportProfile.AmountColumnName)) return;
        _maxStepLevel = MaxStepLevel.Step4;
    }

    private async Task ValidateDataAsync()
    {
        _isValidationRunning = true;
        await SyncPreviewTextToViewModelFileTextAsync(); // Required if PreviewTextArea has not yet lost focus
        _validationErrorMessage = (await _dataContext.ValidateDataAsync()).Message;
        _errorRecordsMudFilter.AvailableItems = _dataContext.ParsedRecords
            .Where(i => !i.IsValid)
            .Select(i => new EquatableCsvMappingError(i.Error))
            .Distinct()
            .ToList();
        _errorRecordsMudFilter.ResetFilter();
        _isValidationRunning = false;
    }

    private async Task ImportDataAsync(bool withoutDuplicates)
    {
        _isImportRunning = true;
        await SyncPreviewTextToViewModelFileTextAsync(); // Required if PreviewTextArea has not yet lost focus
        var importResult = await _dataContext.ImportDataAsync(withoutDuplicates);

        if (importResult.IsSuccessful)
        {
            var parameters = new DialogParameters<DeleteConfirmationDialog>
            {
                { x => x.Title, "Import Transactions" },
                { x => x.Message, $"{importResult.Message} Do you want to clean up your input?" }
            };
            var dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>("Import Transactions", parameters);
            var result = await dialog.Result;
            if (result is { Canceled: false })
            {
                await ClearFormAsync();
            }
        }
        else
        {
            await HandleResult(importResult);
        }

        _isImportRunning = false;
    }

    private async Task ClearFormAsync()
    {
        _dataContext = new ImportPageViewModel(ServiceManager);
        await LoadData();
        if (_fileUpload is not null) await _fileUpload.ClearAsync();
        if (_stepper is not null) await _stepper.ResetAsync();
        _stepperIndex = 0;
        await SyncViewModelFileTextToPreviewTextAsync();
        StateHasChanged();
    }

    private async Task HandleResult(ViewModelOperationResult result)
    {
        if (!result.IsSuccessful)
        {
            var parameters = new DialogParameters<ErrorMessageDialog>
            {
                { x => x.Title, "Import" },
                { x => x.Message, result.Message }
            };
            await DialogService.ShowAsync<ErrorMessageDialog>("Import", parameters);
        }
    }
}
