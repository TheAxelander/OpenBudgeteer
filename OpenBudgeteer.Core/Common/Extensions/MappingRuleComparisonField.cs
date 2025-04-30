namespace OpenBudgeteer.Core.Common.Extensions;

public enum MappingRuleComparisonField
{
    [StringValue("Any")]
    Any = 0,
    [StringValue("Account")]
    Account = 1,
    [StringValue("Payee")]
    Payee = 2,
    [StringValue("Memo")]
    Memo = 3,
    [StringValue("Amount")]
    Amount = 4
}