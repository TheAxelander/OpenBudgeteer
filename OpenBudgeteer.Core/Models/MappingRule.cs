using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Models;

public class MappingRule : BaseObject
{
    private int _mappingRuleId;
    public int MappingRuleId
    {
        get => _mappingRuleId;
        set => Set(ref _mappingRuleId, value);
    }

    private int _bucketRuleSetId;
    [Required]
    public int BucketRuleSetId
    {
        get => _bucketRuleSetId;
        set => Set(ref _bucketRuleSetId, value);
    }

    private int _comparisionField;
    /// <summary>
    /// <see cref="BankTransaction"/> field which should be compared
    /// <para>
    /// 1 - <see cref="BankTransaction.AccountId"/>
    /// 2 - <see cref="BankTransaction.Payee"/>
    /// 3 - <see cref="BankTransaction.Memo"/>
    /// 4 - <see cref="BankTransaction.Amount"/>
    /// </para>
    /// </summary>
    [Required]
    public int ComparisionField
    {
        get => _comparisionField;
        set => Set(ref _comparisionField, value);
    }

    [NotMapped]
    public string ComparisonFieldOutput
    {
        get
        {
            switch (ComparisionField)
            {
                case 1:
                    return nameof(Account);
                case 2:
                    return nameof(BankTransaction.Payee);
                case 3:
                    return nameof(BankTransaction.Memo);
                case 4:
                    return nameof(BankTransaction.Amount);
                default:
                    return string.Empty;
            }
        }
    }

    private int _comparisionType;
    /// <summary>
    /// Identifier how Comparison should happen
    /// <para>
    /// 1 - Equal
    /// 2 - Not Equal
    /// 3 - Contains
    /// 4 - Does not contain
    /// </para>
    /// </summary>
    [Required]
    public int ComparisionType
    {
        get => _comparisionType;
        set => Set(ref _comparisionType, value);
    }

    [NotMapped]
    public string ComparisionTypeOutput
    {
        get
        {
            switch (ComparisionType)
            {
                case 1:
                    return "equal";
                case 2:
                    return "not equal";
                case 3:
                    return "contains";
                case 4:
                    return "does not contain";
                default:
                    return string.Empty;
            }
        }
    }

    private string _comparisionValue;
    /// <summary>
    /// Value of the <see cref="BankTransaction"/> field that needs to be compared
    /// </summary>
    [Required]
    public string ComparisionValue
    {
        get => _comparisionValue;
        set => Set(ref _comparisionValue, value);
    }

}
