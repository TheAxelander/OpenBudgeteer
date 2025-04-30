using System.Collections.Generic;
using System.Linq;

namespace OpenBudgeteer.Blazor.Common;

public static class MultiSelectionTextHelper
{
    public static string GetText(List<string?>? selectedItems, string itemType)
    {
        if (selectedItems is null) return string.Empty;
        return (selectedItems.Count == 1 ? selectedItems.First() : $"{selectedItems.Count} {itemType} selected") ?? string.Empty;
    }
}