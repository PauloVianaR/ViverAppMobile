using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class InverseSbyteToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is sbyte sb)
                return sb == 0;

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? (sbyte)0 : (sbyte)1;
            return (sbyte)1;
        }

    }
}
