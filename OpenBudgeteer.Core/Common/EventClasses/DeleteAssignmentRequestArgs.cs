using OpenBudgeteer.Core.ViewModels;
using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBudgeteer.Core.Common.EventClasses
{
    /// <summary>
    /// Event Handler Argument for requesting the deletion of a bucket assignment
    /// </summary>
    public class DeleteAssignmentRequestArgs : EventArgs
    {
        public PartialBucketViewModelItem Source { get; set; }

        public DeleteAssignmentRequestArgs() { }

        public DeleteAssignmentRequestArgs(PartialBucketViewModelItem source)
        {
            Source = source;
        }
    }
}
