using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public string CurrentFormat { get; set; } = string.Empty;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                string format = string.IsNullOrEmpty(CurrentFormat) ? parameter as string ?? "dd/MM/yyyy" : CurrentFormat;
                return dateTime.ToString(format, culture);
            }

            return value?.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && DateTime.TryParse(str, culture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return DateTime.MinValue;
        }
    }
}
