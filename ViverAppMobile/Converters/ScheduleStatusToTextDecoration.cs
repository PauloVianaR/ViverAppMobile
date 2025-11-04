using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class ScheduleStatusToTextDecoration : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int status && parameter is string confirmParameter)
                return status == int.Parse(confirmParameter) ? TextDecorations.Strikethrough : TextDecorations.None;

            return TextDecorations.None;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
