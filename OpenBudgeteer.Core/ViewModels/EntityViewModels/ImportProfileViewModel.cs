using System;
using OpenBudgeteer.Core.Common;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.ViewModels.EntityViewModels;

public class ImportProfileViewModel : BaseEntityViewModel<ImportProfile>, IEquatable<ImportProfileViewModel>, IComparable<ImportProfileViewModel>
{
    #region Properties & Fields
    
    public enum AdditionalSettingsForCreditValues
    {
        NoSettings = 0,
        CreditInSeparateColumns = 1,
        DebitCreditAlwaysPositive = 2
    }
    
    private Guid _importProfileId;
    /// <summary>
    /// Database Id of the ImportProfile
    /// </summary>
    public Guid ImportProfileId 
    { 
        get => _importProfileId;
        set => Set(ref _importProfileId, value);
    }
    
    private string _profileName;
    /// <summary>
    /// Name of the ImportProfile
    /// </summary>
    public string ProfileName 
    { 
        get => _profileName;
        set => Set(ref _profileName, value);
    }
    
    private AccountViewModel _account;
    /// <summary>
    /// Database Id of the Account assigned to this ImportProfile
    /// </summary>
    public AccountViewModel Account 
    { 
        get => _account;
        set => Set(ref _account, value);
    }
    
    private int _headerRow;
    /// <summary>
    /// Row number on where in the file the headers are defined 
    /// </summary>
    public int HeaderRow 
    { 
        get => _headerRow;
        set => Set(ref _headerRow, value);
    }
    
    private char _delimiter;
    /// <summary>
    /// Delimiter character that is used in the file
    /// </summary>
    public char Delimiter 
    { 
        get => _delimiter;
        set => Set(ref _delimiter, value);
    }
    
    private char _textQualifier;
    /// <summary>
    /// Delimiter character that is used in the file for defining text blocks
    /// </summary>
    public char TextQualifier 
    { 
        get => _textQualifier;
        set => Set(ref _textQualifier, value);
    }
    
    private string _dateFormat;
    /// <summary>
    /// Format of the Date (e.g. yyyy-MM-dd) that is used in the file
    /// </summary>
    public string DateFormat 
    { 
        get => _dateFormat;
        set => Set(ref _dateFormat, value);
    }
    
    private string _numberFormat;
    /// <summary>
    /// Format of the number that is used in the file, use a definition like e.g. en-US
    /// </summary>
    public string NumberFormat 
    { 
        get => _numberFormat;
        set => Set(ref _numberFormat, value);
    }
    
    private string _transactionDateColumnName;
    /// <summary>
    /// Name of the column for property <see cref="BankTransaction.TransactionDate"/>
    /// </summary>
    public string TransactionDateColumnName 
    { 
        get => _transactionDateColumnName;
        set => Set(ref _transactionDateColumnName, value);
    }
    
    private string _payeeColumnName;
    /// <summary>
    /// Name of the column for property <see cref="BankTransaction.Payee"/>
    /// </summary>
    public string PayeeColumnName 
    { 
        get => _payeeColumnName;
        set => Set(ref _payeeColumnName, value);
    }
    
    private string _memoColumnName;
    /// <summary>
    /// Name of the column for property <see cref="BankTransaction.Memo"/>
    /// </summary>
    public string MemoColumnName 
    { 
        get => _memoColumnName;
        set => Set(ref _memoColumnName, value);
    }
    
    private string _amountColumnName;
    /// <summary>
    /// Name of the column for property <see cref="BankTransaction.Amount"/>
    /// </summary>
    public string AmountColumnName 
    { 
        get => _amountColumnName;
        set => Set(ref _amountColumnName, value);
    }
    
    private AdditionalSettingsForCreditValues _additionalSettingCreditValue;
    /// <summary>
    /// Set which kind of special handling applies to Amount column 
    /// </summary>
    public AdditionalSettingsForCreditValues AdditionalSettingCreditValue 
    { 
        get => _additionalSettingCreditValue;
        set => Set(ref _additionalSettingCreditValue, value);
    }

    /// <summary>
    /// Converts <see cref="AdditionalSettingCreditValue"/> for display purposes 
    /// </summary>
    public string AdditionalSettingCreditValueOutput
    {
        get
        {
            return AdditionalSettingCreditValue switch
            {
                AdditionalSettingsForCreditValues.NoSettings => "No special settings for Debit and Credit",
                AdditionalSettingsForCreditValues.CreditInSeparateColumns => "Credit values are in separate columns",
                AdditionalSettingsForCreditValues.DebitCreditAlwaysPositive => "Debit and Credit values are in the same column but always positive",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    private string _creditColumnName;
    /// <summary>
    /// Applies for <see cref="AdditionalSettingsForCreditValues.CreditInSeparateColumns"/>:
    /// Name of the column for property <see cref="BankTransaction.Amount"/> where Credit values are in a separate column
    /// </summary>
    public string CreditColumnName 
    { 
        get => _creditColumnName;
        set => Set(ref _creditColumnName, value);
    }
    
    private string _creditColumnIdentifierColumnName;
    /// <summary>
    /// Applies for <see cref="AdditionalSettingsForCreditValues.DebitCreditAlwaysPositive"/>:
    /// Name of the column that includes a reference showing which records in the file have to be handled as expenses
    /// </summary>
    public string CreditColumnIdentifierColumnName 
    { 
        get => _creditColumnIdentifierColumnName;
        set => Set(ref _creditColumnIdentifierColumnName, value);
    }
    
    private string _creditColumnIdentifierValue;
    /// <summary>
    /// Applies for <see cref="AdditionalSettingsForCreditValues.DebitCreditAlwaysPositive"/>:
    /// Reference value of the column defined in <see cref="CreditColumnIdentifierColumnName"/> which marks a record as expense 
    /// </summary>
    public string CreditColumnIdentifierValue 
    { 
        get => _creditColumnIdentifierValue;
        set => Set(ref _creditColumnIdentifierValue, value);
    }
    
    private bool _additionalSettingAmountCleanup;
    /// <summary>
    /// Should be set to true in case the amount values contain additional characters like the currency 
    /// </summary>
    public bool AdditionalSettingAmountCleanup 
    { 
        get => _additionalSettingAmountCleanup;
        set => Set(ref _additionalSettingAmountCleanup, value);
    }
    
    private string _additionalSettingAmountCleanupValue;
    /// <summary>
    /// A substring or Regex that should be used to cleanup the amount values into pure numbers
    /// </summary>
    public string AdditionalSettingAmountCleanupValue 
    { 
        get => _additionalSettingAmountCleanupValue;
        set => Set(ref _additionalSettingAmountCleanupValue, value);
    }
    
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="ImportProfile"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="importProfile">ImportProfile instance</param>
    protected ImportProfileViewModel(IServiceManager serviceManager, ImportProfile? importProfile) : base(serviceManager)
    {
        if (importProfile is null)
        {
            _importProfileId = Guid.Empty;
            _profileName = string.Empty;
            _account = AccountViewModel.CreateEmpty(serviceManager);
            _headerRow = 0;
            _delimiter = '\0';
            _textQualifier = '\0';
            _dateFormat = string.Empty;
            _numberFormat = string.Empty;
            _transactionDateColumnName = string.Empty;
            _payeeColumnName = string.Empty;
            _memoColumnName = string.Empty;
            _amountColumnName = string.Empty;
            _additionalSettingCreditValue = AdditionalSettingsForCreditValues.NoSettings;
            _creditColumnName = string.Empty;
            _creditColumnIdentifierColumnName = string.Empty;
            _creditColumnIdentifierValue = string.Empty;
            _additionalSettingAmountCleanup = false;
            _additionalSettingAmountCleanupValue = string.Empty;
        }
        else
        {
            _importProfileId = importProfile.Id;
            _profileName = importProfile.ProfileName ?? string.Empty;
            _account = importProfile.AccountId != Guid.Empty
                ? AccountViewModel.CreateFromAccount(serviceManager, serviceManager.AccountService.Get(importProfile.AccountId))
                : AccountViewModel.CreateEmpty(serviceManager);
            _headerRow = importProfile.HeaderRow;
            _delimiter = importProfile.Delimiter;
            _textQualifier = importProfile.TextQualifier;
            _dateFormat = importProfile.DateFormat ?? string.Empty;
            _numberFormat = importProfile.NumberFormat ?? string.Empty;
            _transactionDateColumnName = importProfile.TransactionDateColumnName ?? string.Empty;
            _payeeColumnName = importProfile.PayeeColumnName ?? string.Empty;
            _memoColumnName = importProfile.MemoColumnName ?? string.Empty;
            _amountColumnName = importProfile.AmountColumnName ?? string.Empty;
            _additionalSettingCreditValue = (AdditionalSettingsForCreditValues)importProfile.AdditionalSettingCreditValue;
            _creditColumnName = importProfile.CreditColumnName ?? string.Empty;
            _creditColumnIdentifierColumnName = importProfile.CreditColumnIdentifierColumnName ?? string.Empty;
            _creditColumnIdentifierValue = importProfile.CreditColumnIdentifierValue ?? string.Empty;
            _additionalSettingAmountCleanup = importProfile.AdditionalSettingAmountCleanup;
            _additionalSettingAmountCleanupValue = importProfile.AdditionalSettingAmountCleanupValue ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Initialize a copy of the passed ViewModel
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    protected ImportProfileViewModel(ImportProfileViewModel viewModel) : base(viewModel.ServiceManager)
    {
        _importProfileId = viewModel.ImportProfileId;
        _profileName = viewModel.ProfileName;
        _account = viewModel.Account;
        _headerRow = viewModel.HeaderRow;
        _delimiter = viewModel.Delimiter;
        _textQualifier = viewModel.TextQualifier;
        _dateFormat = viewModel.DateFormat;
        _numberFormat = viewModel.NumberFormat;
        _transactionDateColumnName = viewModel.TransactionDateColumnName;
        _payeeColumnName = viewModel.PayeeColumnName;
        _memoColumnName = viewModel.MemoColumnName;
        _amountColumnName = viewModel.AmountColumnName;
        _additionalSettingCreditValue = viewModel.AdditionalSettingCreditValue;
        _creditColumnName = viewModel.CreditColumnName;
        _creditColumnIdentifierColumnName = viewModel.CreditColumnIdentifierColumnName;
        _creditColumnIdentifierValue = viewModel.CreditColumnIdentifierValue;
        _additionalSettingAmountCleanup = viewModel.AdditionalSettingAmountCleanup;
        _additionalSettingAmountCleanupValue = viewModel.AdditionalSettingAmountCleanupValue;
    }

    /// <summary>
    /// Initialize ViewModel based on an existing <see cref="ImportProfile"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <param name="importProfile">ImportProfile instance</param>
    /// <returns>New ViewModel instance</returns>
    public static ImportProfileViewModel CreateFromImportProfile(IServiceManager serviceManager, ImportProfile importProfile)
    {
        return new ImportProfileViewModel(serviceManager, importProfile);
    }
    
    /// <summary>
    /// Create a copy of this ViewModel to be used for modification
    /// </summary>
    /// <param name="viewModel">Current ViewModel instance</param>
    /// <returns>Copy of current ViewModel instance</returns>
    public static ImportProfileViewModel CreateAsCopy(ImportProfileViewModel viewModel)
    {
        return new ImportProfileViewModel(viewModel);
    }
    
    /// <summary>
    /// Initialize ViewModel used to create a new <see cref="ImportProfile"/> object
    /// </summary>
    /// <param name="serviceManager">Reference to API based services</param>
    /// <returns>New ViewModel instance</returns>
    public static ImportProfileViewModel CreateEmpty(IServiceManager serviceManager)
    {
        return new ImportProfileViewModel(serviceManager, null);
    }

    /// <summary>
    /// Return a deep copy of the ViewModel
    /// </summary>
    public override object Clone()
    {
        return new ImportProfileViewModel(this);
    }

    #endregion
    
    #region Modification Handler
    
    internal override ImportProfile ConvertToDto()
    {
        return new ImportProfile()
        {
            Id = ImportProfileId,
            ProfileName = ProfileName,
            AccountId = Account.AccountId,
            HeaderRow = HeaderRow,
            Delimiter = Delimiter,
            TextQualifier = TextQualifier,
            DateFormat = DateFormat,
            NumberFormat = NumberFormat,
            TransactionDateColumnName = TransactionDateColumnName,
            PayeeColumnName = PayeeColumnName,
            MemoColumnName = MemoColumnName,
            AmountColumnName = AmountColumnName,
            AdditionalSettingCreditValue = (int)AdditionalSettingCreditValue,
            CreditColumnName = CreditColumnName,
            CreditColumnIdentifierColumnName = CreditColumnIdentifierColumnName,
            CreditColumnIdentifierValue = CreditColumnIdentifierValue,
            AdditionalSettingAmountCleanup = AdditionalSettingAmountCleanup,
            AdditionalSettingAmountCleanupValue = AdditionalSettingAmountCleanupValue
        };
    }
    
    /// <summary>
    /// Creates a new <see cref="ImportProfile"/> in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult CreateProfile()
    {
        try
        {
            if (string.IsNullOrEmpty(ProfileName)) 
                throw new Exception("Profile Name must not be empty.");

            ImportProfileId = Guid.Empty;
            var importProfileDto = ConvertToDto();
            ServiceManager.ImportProfileService.Create(importProfileDto);
            ImportProfileId = importProfileDto.Id;
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to create Import Profile: {e.Message}");
        }
    }
    
    /// <summary>
    /// Updates data for the <see cref="ImportProfile"/>  in the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult SaveProfile()
    {
        try
        {
            if (string.IsNullOrEmpty(ProfileName)) 
                throw new Exception("Profile Name must not be empty.");

            ServiceManager.ImportProfileService.Update(ConvertToDto());
            
            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to save Import Profile: {e.Message}");
        }
    }
    
    /// <summary>
    /// Deletes the <see cref="ImportProfile"/> from the database based on ViewModel data
    /// </summary>
    /// <returns>Object which contains information and results of this method</returns>
    public ViewModelOperationResult DeleteProfile()
    {
        try
        {
            ServiceManager.ImportProfileService.Delete(ImportProfileId);

            return new ViewModelOperationResult(true);
        }
        catch (Exception e)
        {
            return new ViewModelOperationResult(false, $"Unable to delete Import Profile: {e.Message}");
        }
    }
    
    #endregion

    #region IEquatable & IComparable Implementation
    
    public bool Equals(ImportProfileViewModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return 
            _importProfileId.Equals(other._importProfileId) && 
            _profileName == other._profileName && 
            _account.Equals(other._account) && 
            _headerRow == other._headerRow && 
            _delimiter == other._delimiter && 
            _textQualifier == other._textQualifier && 
            _dateFormat == other._dateFormat && 
            _numberFormat == other._numberFormat && 
            _transactionDateColumnName == other._transactionDateColumnName && 
            _payeeColumnName == other._payeeColumnName && 
            _memoColumnName == other._memoColumnName && 
            _amountColumnName == other._amountColumnName && 
            _additionalSettingCreditValue == other._additionalSettingCreditValue && 
            _creditColumnName == other._creditColumnName && 
            _creditColumnIdentifierColumnName == other._creditColumnIdentifierColumnName && 
            _creditColumnIdentifierValue == other._creditColumnIdentifierValue && 
            _additionalSettingAmountCleanup == other._additionalSettingAmountCleanup && 
            _additionalSettingAmountCleanupValue == other._additionalSettingAmountCleanupValue;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ImportProfileViewModel)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(_importProfileId);
        hashCode.Add(_profileName);
        hashCode.Add(_account);
        hashCode.Add(_headerRow);
        hashCode.Add(_delimiter);
        hashCode.Add(_textQualifier);
        hashCode.Add(_dateFormat);
        hashCode.Add(_numberFormat);
        hashCode.Add(_transactionDateColumnName);
        hashCode.Add(_payeeColumnName);
        hashCode.Add(_memoColumnName);
        hashCode.Add(_amountColumnName);
        hashCode.Add((int)_additionalSettingCreditValue);
        hashCode.Add(_creditColumnName);
        hashCode.Add(_creditColumnIdentifierColumnName);
        hashCode.Add(_creditColumnIdentifierValue);
        hashCode.Add(_additionalSettingAmountCleanup);
        hashCode.Add(_additionalSettingAmountCleanupValue);
        return hashCode.ToHashCode();
    }
    
    public int CompareTo(ImportProfileViewModel? other)
    {
        return string.Compare(_profileName, other?.ProfileName, StringComparison.Ordinal);
    }

    public override string ToString() => ProfileName;
    
    #endregion
}