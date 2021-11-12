namespace OpenBudgeteer.Core.Models;

public class ImportProfile : BaseObject
{
    private int _importProfileId;
    public int ImportProfileId
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

    private int _accountId;
    public int AccountId
    {
        get => _accountId;
        set => Set(ref _accountId, value);
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
}
