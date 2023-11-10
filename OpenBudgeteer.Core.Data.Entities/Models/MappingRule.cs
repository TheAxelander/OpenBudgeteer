using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class MappingRule : IEntity
{
    [Key, Column("MappingRuleId")]
    public Guid Id { get; set; }

    [Required]
    public Guid BucketRuleSetId { get; set; }

    public BucketRuleSet? BucketRuleSet { get; set; }

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
    public int ComparisionField { get; set; }
    
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
    public int ComparisionType { get; set; }
    
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

    [Required]
    public string ComparisionValue { get; set; } = string.Empty;
}
