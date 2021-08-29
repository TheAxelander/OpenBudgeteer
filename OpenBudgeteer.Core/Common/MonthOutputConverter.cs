using System;
using System.Globalization;

namespace OpenBudgeteer.Core.Common
{
    public class MonthOutputConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int month)) return string.Empty;
            var date = new DateTime(1, month, 1);
            return date.ToString("MMM", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((!(value is string month))) return DateTime.Now.Month;
            for (var i = 1; i < 13; i++)
            {
                var date = new DateTime(1, i, 1);
                if (date.ToString("MMM") == month) return i;
            }
            return DateTime.Now.Month;
        }

        public string ConvertMonth(object value, CultureInfo culture = null)
        {
            return Convert(value, typeof(string), null, culture ?? CultureInfo.CurrentCulture).ToString();
        }
    }
}
