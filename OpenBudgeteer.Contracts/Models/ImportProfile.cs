namespace OpenBudgeteer.Contracts.Models;

public class ImportProfile : BaseObject
{
    private Guid _importProfileId;
    public Guid ImportProfileId
    {
        get => _importProfileId;
        set => Set(ref _importProfileId, value);
    }

    private string _profileName;
    public string ProfileName
    {
        get => _profileName;
        set => Set(ref _profileName, value);
    }

    private Guid _accountId;
    public Guid AccountId
    {
        get => _accountId;
        set => Set(ref _accountId, value);
    }

    private Account _account;

    public Account Account
    {
        get => _account;
        set => Set(ref _account, value);
    }

    private int _headerRow;
    public int HeaderRow
    {
        get => _headerRow;
        set => Set(ref _headerRow, value);
    }

    private char _delimiter;
    public char Delimiter
    {
        get => _delimiter;
        set => Set(ref _delimiter, value);
    }

    private char _textQualifier;
    public char TextQualifier
    {
        get => _textQualifier;
        set => Set(ref _textQualifier, value);
    }

    private string _dateFormat;
    public string DateFormat
    {
        get => _dateFormat;
        set => Set(ref _dateFormat, value);
    }

    private string _numberFormat;
    public string NumberFormat
    {
        get => _numberFormat;
        set => Set(ref _numberFormat, value);
    }

    private string _transactionDateColumnName;
    public string TransactionDateColumnName
    {
        get => _transactionDateColumnName;
        set => Set(ref _transactionDateColumnName, value);
    }

    private string _payeeColumnName;
    public string PayeeColumnName
    {
        get => _payeeColumnName;
        set => Set(ref _payeeColumnName, value);
    }

    private string _memoColumnName;
    public string MemoColumnName
    {
        get => _memoColumnName;
        set => Set(ref _memoColumnName, value);
    }

    private string _amountColumnName;
    public string AmountColumnName
    {
        get => _amountColumnName;
        set => Set(ref _amountColumnName, value);
    }

    private int _additionalSettingCreditValue;
    /// <summary>
    /// Parameter how Credit values can be mapped:
    /// <para>
    /// 0 - No special mapping<br />
    /// 1 - Credit values are in separate columns<br />
    /// 2 - Debit and Credit values are in the same column but always positive<br />
    /// </para>
    /// </summary>
    public int AdditionalSettingCreditValue
    {
        get => _additionalSettingCreditValue;
        set => Set(ref _additionalSettingCreditValue, value);
    }

    private string _creditColumnName;
    /// <remarks>
    /// Only applies for <see cref="AdditionalSettingCreditValue"/> = 1
    /// </remarks>
    public string CreditColumnName
    {
        get => _creditColumnName;
        set => Set(ref _creditColumnName, value);
    }

    private string _creditColumnIdentifierColumnName;
    /// <summary>
    /// Contains the name of the column which identifies a credit record
    /// </summary>
    /// <remarks>
    /// Only applies for <see cref="AdditionalSettingCreditValue"/> = 2
    /// </remarks>
    public string CreditColumnIdentifierColumnName
    {
        get => _creditColumnIdentifierColumnName;
        set => Set(ref _creditColumnIdentifierColumnName, value);
    }

    private string _creditColumnIdentifierValue;
    /// <summary>
    /// Contains the value of the column from <see cref="CreditColumnIdentifierColumnName"/> 
    /// which identifies a credit record
    /// </summary>
    /// <remarks>
    /// Only applies for <see cref="AdditionalSettingCreditValue"/> = 2
    /// </remarks>
    public string CreditColumnIdentifierValue
    {
        get => _creditColumnIdentifierValue;
        set => Set(ref _creditColumnIdentifierValue, value);
    }

    private bool _additionalSettingAmountCleanup;
    /// <summary>
    /// Parameter if Amount values contain additional characters which need to be cleaned up during mapping
    /// </summary>
    public bool AdditionalSettingAmountCleanup
    {
        get => _additionalSettingAmountCleanup;
        set => Set(ref _additionalSettingAmountCleanup, value);
    }

    private string _additionalSettingAmountCleanupValue;
    /// <summary>
    /// Contains the substring that needs to be cleaned up from Amount values during mapping
    /// </summary>
    /// <remarks>
    /// Only applies if <see cref="AdditionalSettingAmountCleanup"/> = true
    /// </remarks>
    public string AdditionalSettingAmountCleanupValue
    {
        get => _additionalSettingAmountCleanupValue;
        set => Set(ref _additionalSettingAmountCleanupValue, value);
    }
}
