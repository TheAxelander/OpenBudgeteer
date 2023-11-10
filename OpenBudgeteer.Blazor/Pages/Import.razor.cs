using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.ViewModels.PageViewModels;

namespace OpenBudgeteer.Blazor.Pages;

public partial class Import : ComponentBase
{
    [Inject] private IServiceManager ServiceManager { get; set; } = null!;

    private ImportPageViewModel _dataContext = null!;

    private ElementReference _inputElement;
    private ElementReference _step1AccordionButtonElement;
    private ElementReference _step4AccordionButtonElement;

    private readonly Guid PLACEHOLDER_ITEM_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string PLACEHOLDER_ITEM_VALUE = "___PlaceholderItem___";
    private const string DUMMY_COLUMN = "---Select Column---";

    private readonly ImportProfile _dummyImportProfile = new() 
    { 
        Id = Guid.Empty, 
        ProfileName = "---Select Import Profile---",
        AccountId = Guid.Empty
    };

    private readonly Core.Data.Entities.Models.Account _dummyAccount = new() 
    { 
        Id = Guid.Empty, 
        Name = "---Select Target Account---" 
    };

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
        _dataContext.AvailableImportProfiles.Insert(0, _dummyImportProfile);
        _dataContext.AvailableAccounts.Insert(0, _dummyAccount);
        _dataContext.SelectedImportProfile = _dummyImportProfile;
        _dataContext.SelectedAccount = _dummyAccount;
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
            var profile = _dataContext.AvailableImportProfiles.FirstOrDefault(i => i.ProfileName == profileName, _dummyImportProfile);
            _dataContext.SelectedImportProfile = profile;
            SelectedImportProfile_SelectionChanged(profile.Id.ToString()); // Dirty solution, to be replaced by API in future
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
        _step2Enabled = false;
        _step3Enabled = false;
        _step4Enabled = false;
        _dataContext.SelectedImportProfile = _dummyImportProfile;
        _dataContext.SelectedAccount = _dummyAccount;

        var file = (await FileReaderService.CreateReference(_inputElement).EnumerateFilesAsync()).FirstOrDefault();
        if (file == null) return;
        HandleResult(await _dataContext.HandleOpenFileAsync(await file.OpenReadAsync()));
        _step2Enabled = true;
    }
    
    private void LoadProfile()
    {
        _dataContext.InitializeDataFromImportProfile();
        _step3Enabled = 
            _dataContext.SelectedImportProfile.Id != Guid.Empty && 
            _dataContext.SelectedImportProfile.Id != PLACEHOLDER_ITEM_ID;
        _dataContext.IdentifiedColumns.Insert(0, DUMMY_COLUMN);
        CheckColumnMapping();
        StateHasChanged();
    }

    private void DeleteProfile()
    {
        _isDeleteConfirmationDialogVisible = false;
        HandleResult(_dataContext.DeleteProfile());
        if (_dataContext.SelectedImportProfile.Id == Guid.Empty ||
            _dataContext.SelectedImportProfile.Id == PLACEHOLDER_ITEM_ID)
        {
            _dataContext.SelectedImportProfile = _dummyImportProfile;
            _dataContext.AvailableImportProfiles.Insert(0, _dummyImportProfile);
            _dataContext.SelectedAccount = _dummyAccount;
        }
    }

    private void LoadHeaders()
    {
        var result = _dataContext.LoadHeaders();
        if (result.IsSuccessful)
        {
            _dataContext.IdentifiedColumns.Insert(0, DUMMY_COLUMN);
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
            _dataContext.SelectedImportProfile.TransactionDateColumnName == PLACEHOLDER_ITEM_VALUE) return;
        // Make Payee optional
        //if (string.IsNullOrEmpty(_dataContext.PayeeColumn) || _dataContext.PayeeColumn == PLACEHOLDER_ITEM_VALUE) return;
        if (string.IsNullOrEmpty(_dataContext.SelectedImportProfile.MemoColumnName) || 
            _dataContext.SelectedImportProfile.MemoColumnName == PLACEHOLDER_ITEM_VALUE) return;
        if (string.IsNullOrEmpty(_dataContext.SelectedImportProfile.AmountColumnName) || 
            _dataContext.SelectedImportProfile.AmountColumnName == PLACEHOLDER_ITEM_VALUE) return;
        _step4Enabled = true;
    }

    private async Task ValidateDataAsync()
    {
        _isValidationRunning = true;
        _dataContext.IdentifiedColumns.Remove(DUMMY_COLUMN); // Remove DummyColumn to prevent wrong column index 
        _validationErrorMessage = (await _dataContext.ValidateDataAsync()).Message;
        _dataContext.IdentifiedColumns.Insert(0, DUMMY_COLUMN);
        _isValidationRunning = false;
    }

    private async Task ImportDataAsync(bool withoutDuplicates)
    {
        _isImportRunning = true;
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
        _forceShowStep1 = true;
        StateHasChanged();
    }
    
    private void SelectedImportProfile_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _dataContext.SelectedImportProfile = _dataContext.AvailableImportProfiles
            .FirstOrDefault(i => i.Id == Guid.Parse(value), _dummyImportProfile); 
        _step3Enabled = false;
        _step4Enabled = false;
        if (_dataContext.SelectedImportProfile.Id != PLACEHOLDER_ITEM_ID) LoadProfile();
    }

    private void TargetAccount_SelectionChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _dataContext.SelectedAccount = _dataContext.AvailableAccounts.FirstOrDefault(i => i.Id == Guid.Parse(value), _dummyAccount);
        _dataContext.SelectedImportProfile.AccountId = _dataContext.SelectedAccount.Id;
    }

    private void ColumnMapping_SelectionChanged(string? value, MappingColumn mappingColumn)
    {
        if (string.IsNullOrEmpty(value)) return;
        switch (mappingColumn)
        {
            case MappingColumn.TransactionDate:
                _dataContext.SelectedImportProfile.TransactionDateColumnName = value;
                CheckColumnMapping();
                break;
            case MappingColumn.Payee:
                _dataContext.SelectedImportProfile.PayeeColumnName = value;
                break;
            case MappingColumn.Memo:
                _dataContext.SelectedImportProfile.MemoColumnName = value; 
                CheckColumnMapping();
                break;
            case MappingColumn.Amount:
                _dataContext.SelectedImportProfile.AmountColumnName = value; 
                CheckColumnMapping();
                break;
            case MappingColumn.Credit:
                _dataContext.SelectedImportProfile.CreditColumnName = value;
                break;
            case MappingColumn.CreditColumnIdentifier:
                _dataContext.SelectedImportProfile.CreditColumnIdentifierColumnName = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mappingColumn), mappingColumn, null);
        }
    }

    private void AdditionalSettingCreditValue_SelectionChanged(ChangeEventArgs e)
    {
        var value = Convert.ToInt32(e.Value);
        _dataContext.SelectedImportProfile.AdditionalSettingCreditValue = value;
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
        _isInfoDialogVisible = true;
    }
}