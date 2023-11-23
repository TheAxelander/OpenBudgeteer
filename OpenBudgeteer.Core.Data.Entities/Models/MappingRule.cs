using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class MappingRule : IEntity
{
    [Key, Column("MappingRuleId")]
    public Guid Id { get; set; }

    [Required]
    public Guid BucketRuleSetId { get; set; }

    [JsonIgnore]
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
    
    [Required]
    public string ComparisionValue { get; set; } = null!;
}
