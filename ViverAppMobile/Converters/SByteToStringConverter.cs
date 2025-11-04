using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SByteToStringConverter : IValueConverter
    {
        public string TextForTrue { get; set; } = "True";
        public string TextForFalse { get; set; } = "False";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is sbyte sb && sb == 1)
                return TextForTrue;

            return TextForFalse;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text == TextForTrue) return (sbyte)1;
                if (text == TextForFalse) return (sbyte)0;
            }
            return (sbyte)0;
        }

    }
}
