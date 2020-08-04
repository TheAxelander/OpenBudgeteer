using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenBudgeteer.Core.Common
{
    public class MonthOutputConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int month)) return string.Empty;
            var date = new DateTime(1, month, 1);
            return date.ToString("MMM");
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

        public string ConvertMonth(object value)
        {
            return Convert(value, typeof(string), null, CultureInfo.CurrentCulture).ToString();
        }
    }
}
