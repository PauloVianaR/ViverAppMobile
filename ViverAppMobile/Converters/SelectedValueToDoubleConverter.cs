using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SelectedValueToDoubleConverter : IValueConverter
    {
        public double SelectedValue { get; set; } = 0.5;
        public double UnselectedValue { get; set; } = 1;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.Equals(parameter))
                return SelectedValue;

            return UnselectedValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d == SelectedValue) return parameter;
                if (d == UnselectedValue) return null;
            }
            return null;
        }

    }
}
