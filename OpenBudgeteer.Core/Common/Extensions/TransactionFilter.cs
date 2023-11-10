using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.Common.Extensions;

/// <summary>
/// Identifier which kind of filter can be applied on the <see cref="TransactionViewModel"/> 
/// </summary>
public enum TransactionFilter: int
{
    [StringValue("No Filter")]
    NoFilter = 0, 
    [StringValue("Hide mapped")]
    HideMapped = 1, 
    [StringValue("Only mapped")]
    OnlyMapped = 2,
    [StringValue("In Modification")]
    InModification = 3,
}