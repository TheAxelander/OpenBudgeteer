using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenBudgeteer.Core.ViewModels.EntityViewModels;

namespace OpenBudgeteer.Blazor.Common;

public class MudComparer : IComparer<object>
{
    public int Compare(object? x, object? y)
    {
        // Check if the objects are null
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        
        // Use IComparable implementation
        if (x is IComparable && y is IComparable) return ((IComparable)x).CompareTo(y);

        // Cover special case of sorting Buckets from Transaction Page
        if (x is ObservableCollection<PartialBucketViewModel> partialBucketsX && 
            y is ObservableCollection<PartialBucketViewModel> partialBucketsY)
        {
            var sortedX = partialBucketsX.ToList().Order().FirstOrDefault();
            var sortedY = partialBucketsY.ToList().Order().FirstOrDefault();
            return Compare(sortedX, sortedY);
        }
        
        // Cover special case of sorting Mapping Rules from Rules Page
        if (x is ObservableCollection<MappingRuleViewModel> mappingRulesX && 
            y is ObservableCollection<MappingRuleViewModel> mappingRulesY)
        {
            var sortedX = mappingRulesX.ToList().Order().FirstOrDefault();
            var sortedY = mappingRulesY.ToList().Order().FirstOrDefault();
            return Compare(sortedX, sortedY);
        }
        
        // Fallback to String based comparison
        return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
    }
}