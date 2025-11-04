using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public string TextForTrue { get; set; } = "True";
        public string TextForFalse { get; set; } = "False";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool vl)
                return vl ? TextForTrue : TextForFalse;

            return TextForFalse;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text == TextForTrue) return true;
                if (text == TextForFalse) return false;
            }
            return false;
        }

    }
}
