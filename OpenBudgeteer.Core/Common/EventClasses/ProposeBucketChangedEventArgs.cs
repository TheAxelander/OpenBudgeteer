using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBudgeteer.Core.Common.EventClasses
{
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
}
