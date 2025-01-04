using System.Drawing;

namespace OpenBudgeteer.Blazor.Common;

public static class ColorExtensions
{
    public static string ToHexString(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}