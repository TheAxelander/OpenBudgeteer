using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBudgeteer.Core.Data.Entities.Models;

public class ImportProfile : IEntity
{
    [Key, Column("ImportProfileId")]
    public Guid Id { get; set; }

    public string ProfileName { get; set; } = string.Empty;

    public Guid AccountId { get; set; }

    public Account? Account { get; set; }

    public int HeaderRow { get; set; }

    public char Delimiter { get; set; }

    public char TextQualifier { get; set; }

    public string? DateFormat { get; set; }

    public string? NumberFormat { get; set; }

    public string? TransactionDateColumnName { get; set; }

    public string? PayeeColumnName { get; set; }

    public string? MemoColumnName { get; set; }

    public string? AmountColumnName { get; set; }

    public int AdditionalSettingCreditValue { get; set; }

    public string? CreditColumnName { get; set; }

    public string? CreditColumnIdentifierColumnName { get; set; }

    public string? CreditColumnIdentifierValue { get; set; }

    public bool AdditionalSettingAmountCleanup { get; set; }

    public string? AdditionalSettingAmountCleanupValue { get; set; }
}
