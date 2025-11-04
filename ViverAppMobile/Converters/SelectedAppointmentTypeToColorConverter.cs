using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverAppMobile.Models;

namespace ViverAppMobile.Converters
{
    public class SelectedAppointmentTypeToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is AppointmentType current && Enum.TryParse(parameter?.ToString(), out AppointmentType target))
            {
                return current == target ? Colors.White : Color.FromArgb("#DDDDDD");
            }

            return Color.FromArgb("#DDDDDD");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color && Enum.TryParse(parameter?.ToString(), out AppointmentType target))
            {
                if (color == Colors.White) return target;
                return null;
            }
            return null;
        }

    }
}
