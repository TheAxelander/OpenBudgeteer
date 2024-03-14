namespace OpenBudgeteer.Core.Common.Extensions;

public enum MappingRuleComparisonField
{
    [StringValue("Account")]
    Account = 1,
    [StringValue("Payee")]
    Payee = 2,
    [StringValue("Memo")]
    Memo = 3,
    [StringValue("Amount")]
    Amount = 4
}