using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class SByteToColorConverter : IValueConverter
    {
        public Color SelectedColor { get; set; } = Colors.Purple;
        public Color UnselectedColor { get; set; } = Colors.Blue;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is sbyte sb && sb == (sbyte)1)
                return SelectedColor;

            return UnselectedColor;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (color == SelectedColor) return (sbyte)1;
                if (color == UnselectedColor) return (sbyte)0;
            }
            return (sbyte)0;
        }

    }
}
