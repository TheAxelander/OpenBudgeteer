using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.Common.Extensions;

/// <summary>
/// Identifier which kind of recurrence can be selected for a RecurringTransaction in <see cref="RecurringTransactionViewModel"/> 
/// </summary>
public enum RecurringTransactionRecurrenceType
{
    [StringValue("Weeks")]
    Weeks = 1, 
    [StringValue("Months")]
    Months = 2, 
    [StringValue("Quarters")]
    Quarters = 3,
    [StringValue("Years")]
    Years = 4,
}