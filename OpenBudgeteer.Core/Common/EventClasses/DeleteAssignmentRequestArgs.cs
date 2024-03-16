using System;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Core.Common.EventClasses;

/// <summary>
/// Event Handler Argument for requesting the deletion of a bucket assignment
/// </summary>
public class DeleteAssignmentRequestArgs : EventArgs
{
    public PartialBucketViewModel Source { get; private set; }

    public DeleteAssignmentRequestArgs(PartialBucketViewModel source)
    {
        Source = source;
    }
}
