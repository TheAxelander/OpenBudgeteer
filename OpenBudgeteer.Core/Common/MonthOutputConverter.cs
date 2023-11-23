using System;
using System.Globalization;

namespace OpenBudgeteer.Core.Common;

public class MonthOutputConverter
{
    public string Convert(int value, CultureInfo culture)
    {
        var date = new DateTime(1, value, 1);
        return date.ToString("MMM", culture);
    }

    public int ConvertBack(string value, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(value)) return DateTime.Now.Month;
        for (var i = 1; i < 13; i++)
        {
            var date = new DateTime(1, i, 1);
            if (date.ToString("MMM", culture) == value) return i;
        }
        return DateTime.Now.Month;
    }

    public string ConvertMonth(int value, CultureInfo culture)
    {
        return Convert(value, culture);
    }
}
