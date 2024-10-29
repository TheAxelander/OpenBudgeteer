using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using OpenBudgeteer.Blazor.Common.InputLargeTextArea;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Import : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private ImportPageViewModel _dataContext = null!;

    private ElementReference _inputElement;
    private ElementReference _step1AccordionButtonElement;
    private ElementReference _step4AccordionButtonElement;
    
    private InputLargeTextArea? _previewTextArea;
    private CancellationTokenSource? _previewOnChangeCancellationTokenSource;

    //private const string DUMMY_COLUMN = "---Select Column---";

    //private ImportProfileViewModel _dummyImportProfile;
    
    private bool _step2Enabled;
    private bool _step3Enabled;
    private bool _step4Enabled;
    private bool _forceShowStep1;
    private bool _forceShowStep4;

    private enum MappingColumn
    {
        TransactionDate, Payee, Memo, Amount, Credit, CreditColumnIdentifier
    }

    private bool _isValidationRunning;
    private bool _isImportRunning;

    private string _validationErrorMessage = string.Empty;

    private bool _isConfirmationModalDialogVisible;
    private string _importConfirmationMessage = string.Empty;

    private bool _isInfoDialogVisible;
    private bool _isInfoDialogInteractionEnabled;
    private string _infoDialogMessage = string.Empty;

    private bool _isDeleteConfirmationDialogVisible;

    private bool _isErrorModalDialogVisible;
    private string _errorModalDialogMessage = string.Empty;

    protected override void OnInitialized()
    {
        _dataContext = new ImportPageViewModel(ServiceManager);
        
        LoadData();
        LoadFromQueryParams();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_forceShowStep1)
        {
            _forceShowStep1 = false;
            await JSRuntime.InvokeVoidAsync("ImportPage.triggerClick", _step1AccordionButtonElement);
        }
        if (_forceShowStep4)
        {
            _forceShowStep4 = false;
            await JSRuntime.InvokeVoidAsync("ImportPage.triggerClick", _step4AccordionButtonElement);
        }
    }

    private void LoadData()
    {
        HandleResult(_dataContext.LoadData());
        
        _step2Enabled = false;
        _step3Enabled = false;
        _step4Enabled = false;
    }

    private async void LoadFromQueryParams()
    {
        var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        if (query.TryGetValue("csv", out var csv64))
        {
            HandleResult(await LoadCsvFromBase64StringAsync(csv64!));
        }

        if (_step2Enabled && query.TryGetValue("profile", out var profileName))
        {
            var profile = _dataContext.AvailableImportProfiles.First(i => i.ProfileName == profileName);
            _dataContext.SelectedImportProfile = profile;
            SelectedImportProfile_SelectionChanged(profile.ImportProfileId.ToString()); // Dirty solution, to be replaced by API in future
        }

        if (_step4Enabled)
        {
            await ValidateDataAsync();
            _forceShowStep4 = true;
            StateHasChanged();
        }
    }

    private async Task<ViewModelOperationResult> LoadCsvFromBase64StringAsync(string csv64)
    {
        try
        {
            var csv = Encoding.UTF8.GetString(Convert.FromBase64String(csv64));
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(csv);
            await writer.FlushAsync();
            stream.Position = 0;
            var res = await _dataContext.HandleOpenFileAsync(stream);
            if (res.IsSuccessful)
            {
                await SyncViewModelFileTextToPreviewTextAsync();
                _step2Enabled = true;
            }
            return res;
        }
        catch (Exception e)
        {
             return new ViewModelOperationResult(false, $"Failed to load CSV: {e.Message}");
        }
    }

    private async Task ReadFileAsync()
    {
        LoadData();

        _infoDialogMessage = "Uploading and processing file...";
        _isInfoDialogInteractionEnabled = false;
        _isInfoDialogVisible = true;
        
        var file = (await FileReaderService.CreateReference(_inputElement).EnumerateFilesAsync()).FirstOrDefault();
        if (file == null) return;
        HandleResult(await _dataContext.HandleOpenFileAsync(await file.OpenReadAsync()));
        await SyncViewModelFileTextToPreviewTextAsync();
        
        _isInfoDialogVisible = false;
        _step2Enabled = true;
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
    
    private void LoadProfile()
    {
        _dataContext.ResetLoadFigures();
        _step3Enabled = _dataContext.SelectedImportProfile.ImportProfileId != Guid.Empty;
        CheckColumnMapping();
        StateHasChanged();
    }

    private void DeleteProfile()
    {
        _isDeleteConfirmationDialogVisible = false;
        HandleResult(_dataContext.DeleteProfile());
        LoadData();
        _step2Enabled = true;
    }

    private void LoadHeaders()
    {
        var result = _dataContext.LoadHeaders();
        if (result.IsSuccessful)
        {
            _step3Enabled = true;
        }
        else
        {
            HandleResult(result);
        }
    }

    private void CheckColumnMapping()
    {
        _step4Enabled = false;
        if (string.IsNullOrEmpty(_dataContext.SelectedImportProfile.TransactionDateColumnName) || 
            _dataContext.SelectedImportProfile.TransactionDateColumnName == ImportPageViewModel.DummyColumn) return;
        // Make Payee optional
        //if (string.IsNullOrEmpty(_dataContext.PayeeColumn) || _dataContext.PayeeColumn == PLACEHOLDER_ITEM_VALUE) return;
        if (string.IsNullOrEmpty(_dataContext.SelectedImportProfile.MemoColumnName) || 
            _dataContext.SelectedImportProfile.MemoColumnName == ImportPageViewModel.DummyColumn) return;
        if (string.IsNullOrEmpty(_dataContext.SelectedImportProfile.AmountColumnName) || 
            _dataContext.SelectedImportProfile.AmountColumnName == ImportPageViewModel.DummyColumn) return;
        _step4Enabled = true;
    }

    private async Task ValidateDataAsync()
    {
        _isValidationRunning = true;
        await SyncPreviewTextToViewModelFileTextAsync(); // Required if PreviewTextArea has not yet lost focus
        _validationErrorMessage = (await _dataContext.ValidateDataAsync()).Message;
        _isValidationRunning = false;
    }

    private async Task ImportDataAsync(bool withoutDuplicates)
    {
        _isImportRunning = true;
        await SyncPreviewTextToViewModelFileTextAsync(); // Required if PreviewTextArea has not yet lost focus
        var result = await _dataContext.ImportDataAsync(withoutDuplicates);
        _importConfirmationMessage = result.Message;
        _isImportRunning = false;
        _isConfirmationModalDialogVisible = true;
    }

    private async Task ClearFormAsync()
    {
        _isConfirmationModalDialogVisible = false;
        _step2Enabled = false;
        _step3Enabled = false;
        _step4Enabled = false;
        await FileReaderService.CreateReference(_inputElement).ClearValue();
        _dataContext = new ImportPageViewModel(ServiceManager);
        LoadData();
        await SyncViewModelFileTextToPreviewTextAsync();
        _forceShowStep1 = true;
        StateHasChanged();
    }
    
    private void SelectedImportProfile_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        var selection = _dataContext.AvailableImportProfiles
            .First(i => i.ImportProfileId == Guid.Parse(value));
        // This copy prevents on-the-fly updates e.g. on Profile Name for AvailableImportProfiles
        _dataContext.SelectedImportProfile = ImportProfileViewModel.CreateAsCopy(selection); 
        _step3Enabled = false;
        _step4Enabled = false;
        if (_dataContext.SelectedImportProfile.ImportProfileId != Guid.Empty) LoadProfile();
    }

    private void TargetAccount_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _dataContext.SelectedImportProfile.Account = 
            _dataContext.AvailableAccounts.First(i => i.AccountId == Guid.Parse(value));
    }

    private void ColumnMapping_SelectionChanged(string? value, MappingColumn mappingColumn)
    {
        if (string.IsNullOrEmpty(value)) return;
        var newValue = value != ImportPageViewModel.DummyColumn ? value : string.Empty;
        switch (mappingColumn)
        {
            case MappingColumn.TransactionDate:
                _dataContext.SelectedImportProfile.TransactionDateColumnName = newValue;
                CheckColumnMapping();
                break;
            case MappingColumn.Payee:
                _dataContext.SelectedImportProfile.PayeeColumnName = newValue;
                break;
            case MappingColumn.Memo:
                _dataContext.SelectedImportProfile.MemoColumnName = newValue; 
                CheckColumnMapping();
                break;
            case MappingColumn.Amount:
                _dataContext.SelectedImportProfile.AmountColumnName = newValue; 
                CheckColumnMapping();
                break;
            case MappingColumn.Credit:
                _dataContext.SelectedImportProfile.CreditColumnName = newValue;
                break;
            case MappingColumn.CreditColumnIdentifier:
                _dataContext.SelectedImportProfile.CreditColumnIdentifierColumnName = newValue;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mappingColumn), mappingColumn, null);
        }
    }

    private void AdditionalSettingCreditValue_SelectionChanged(ChangeEventArgs e)
    {
        var value = Convert.ToInt32(e.Value);
        _dataContext.SelectedImportProfile.AdditionalSettingCreditValue = (ImportProfileViewModel.AdditionalSettingsForCreditValues)value;
    }

    private void HandleResult(ViewModelOperationResult result, string successMessage = "")
    {
        if (!result.IsSuccessful)
        {
            _errorModalDialogMessage = result.Message;
            _isErrorModalDialogVisible = true;
            return;
        }
        if (string.IsNullOrEmpty(successMessage)) return;

        _infoDialogMessage = successMessage;
        _isInfoDialogInteractionEnabled = true;
        _isInfoDialogVisible = true;
    }
}