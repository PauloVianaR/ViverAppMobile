using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SignalToStringConverter : IValueConverter
    {
        public string StringForPositive { get; set; } = "\ue83b";
        public string StringForNegative { get; set; } = "\ue83a";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return StringForPositive;

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, culture, out var number))
            {
                return number >= 0 ? StringForPositive : StringForNegative;
            }

            return StringForPositive;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str == StringForPositive)
                    return 0;
                if (str == StringForNegative)
                    return -1;
            }

            return null;
        }
    }
}
