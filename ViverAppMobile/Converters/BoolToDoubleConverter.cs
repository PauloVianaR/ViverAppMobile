using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class BoolToDoubleConverter : IValueConverter
    {
        public double DoubleForTrue { get; set; } = 0;
        public double DoubleForFalse { get; set; } = 1;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? DoubleForTrue : DoubleForFalse;

            return DoubleForFalse;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double i)
                return i == DoubleForTrue;

            if (value is string s && double.TryParse(s, out double parsed))
                return parsed == DoubleForTrue;

            return false;
        }
    }
}
