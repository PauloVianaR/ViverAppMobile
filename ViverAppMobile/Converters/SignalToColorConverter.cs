using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SignalToColorConverter : IValueConverter
    {
        public Color ColorForPositive { get; set; } = Colors.Green;
        public Color ColorForNegative { get; set; } = Colors.Red;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return ColorForPositive;

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, culture, out var number))
            {
                return number >= 0 ? ColorForPositive : ColorForNegative;
            }

            return ColorForPositive;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (color == ColorForPositive)
                    return 0;
                if (color == ColorForNegative)
                    return -1;
            }

            return null;
        }

    }
}
