namespace OpenBudgeteer.Core.Common.Extensions;

public enum MappingRuleComparisonType
{
    [StringValue("Equal")]
    Equal = 1,
    [StringValue("Not equal")]
    NotEqual = 2,
    [StringValue("Contains")]
    Contains = 3,
    [StringValue("Does not contain")]
    DoesNotContain = 4
}