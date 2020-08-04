using OpenBudgeteer.Core.ViewModels;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBudgeteer.Core.Common.EventClasses
{
    /// <summary>
    /// Event Handler Argument for setting the amount in <see cref="PartialBucketViewModelItem"/>
    /// </summary>
    public class AmountChangedArgs : EventArgs
    {
        public PartialBucketViewModelItem Source { get; set; }

        public decimal NewAmount { get; set; }

        public AmountChangedArgs() { }

        public AmountChangedArgs(PartialBucketViewModelItem source, decimal newAmount)
        {
            Source = source;
            NewAmount = newAmount;
        }
    }
}
