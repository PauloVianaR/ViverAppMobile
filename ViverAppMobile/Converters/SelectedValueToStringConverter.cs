using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SelectedValueToStringConverter : IValueConverter
    {
        public string? TextForTrue { get; set; } = string.Empty;
        public string? TextForFalse { get; set; } = string.Empty;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.Equals(parameter))
                return TextForTrue;

            return TextForFalse;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text == TextForTrue) return parameter;
                if (text == TextForFalse) return null;
            }
            return null;
        }

    }
}
