using OpenBudgeteer.Core.ViewModels.ItemViewModels;
using System;

namespace OpenBudgeteer.Core.Common.EventClasses;

/// <summary>
/// Event Handler Argument for requesting the deletion of a bucket assignment
/// </summary>
public class DeleteAssignmentRequestArgs : EventArgs
{
    public PartialBucketViewModelItem Source { get; private set; }

    public DeleteAssignmentRequestArgs(PartialBucketViewModelItem source)
    {
        Source = source;
    }
}
