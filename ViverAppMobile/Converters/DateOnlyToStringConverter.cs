using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class DateOnlyToStringConverter : IValueConverter
    {
        public string CurrentFormat { get; set; } = string.Empty;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateOnly dateOnly)
                return this.ConvertToCurrentFormat(dateOnly, parameter, culture);

            if (value is DateTime dateTime)
            {
                var dateonly = DateOnly.FromDateTime(dateTime);
                return this.ConvertToCurrentFormat(dateonly, parameter, culture);
            }

            return value?.ToString() ?? string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && DateOnly.TryParse(str, culture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return DateOnly.MinValue;
        }

        private object? ConvertToCurrentFormat(DateOnly dateOnly, object? parameter, CultureInfo culture)
        {
            if (CurrentFormat == "age")
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                int age = today.Year - dateOnly.Year;
                if (today < dateOnly.AddYears(age))
                    age--;

                age = Math.Max(age, 0);
                string pluralSufix = age != 1 ? "s" : string.Empty;

                return $"{age} ano{pluralSufix}";
            }

            string format = string.IsNullOrEmpty(CurrentFormat) ? parameter as string ?? "dd/MM/yyyy" : CurrentFormat;

            return dateOnly.ToString(format, culture);
        }
    }
}
