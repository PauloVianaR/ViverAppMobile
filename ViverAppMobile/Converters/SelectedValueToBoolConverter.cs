using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SelectedValueToBoolConverter : IValueConverter
    {
        public bool Inverse { get; set; } = false;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.Equals(parameter))
                return value.Equals(parameter) != Inverse;

            return Inverse;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b != Inverse ? parameter : null;
            }
            return null;
        }

    }
}
