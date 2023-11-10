using System;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.Common.EventClasses;

/// <summary>
/// Event Handler Argument for setting the amount in <see cref="PartialBucketViewModelItem"/>
/// </summary>
public class AmountChangedArgs : EventArgs
{
    public PartialBucketViewModel Source { get; private set; }

    public decimal NewAmount { get; private set; }

    public AmountChangedArgs(PartialBucketViewModel source, decimal newAmount)
    {
        Source = source;
        NewAmount = newAmount;
    }
}
