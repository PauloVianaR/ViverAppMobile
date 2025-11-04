using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class TabColorConverter : IValueConverter
    {
        public Color ColorForTrue { get; set; } = Colors.CornflowerBlue;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string tabIndexStr && int.TryParse(tabIndexStr, out int tabIndex))
            {
                return selectedIndex == tabIndex ? ColorForTrue : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
