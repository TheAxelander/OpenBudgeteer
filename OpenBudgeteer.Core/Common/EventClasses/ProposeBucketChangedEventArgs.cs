using System;

namespace OpenBudgeteer.Core.Common.EventClasses;

public class ProposeBucketChangedEventArgs : EventArgs
{
    public int NewValue { get; private set; }
    public int NewProgress { get; private set; }

    public ProposeBucketChangedEventArgs(int newValue, int newProgress)
    {
        NewValue = newValue;
        NewProgress = newProgress;
    }
}
