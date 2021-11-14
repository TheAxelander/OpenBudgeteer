using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using System;

namespace OpenBudgeteer.Core.Common.EventClasses;

/// <summary>
/// Event Handler Argument for setting the amount in <see cref="PartialBucketViewModelItem"/>
/// </summary>
public class AmountChangedArgs : EventArgs
{
    public PartialBucketViewModelItem Source { get; private set; }

    public decimal NewAmount { get; private set; }

    public AmountChangedArgs(PartialBucketViewModelItem source, decimal newAmount)
    {
        Source = source;
        NewAmount = newAmount;
    }
}
