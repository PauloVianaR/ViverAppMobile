using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Converters
{
    public class ScheduleStatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return (ScheduleStatus)status switch
                {
                    ScheduleStatus.Confirmed => Color.FromArgb("#dcfce8"),
                    ScheduleStatus.Pending => Colors.LightGoldenrodYellow,
                    ScheduleStatus.Concluded => Colors.CornflowerBlue,
                    ScheduleStatus.Canceled => Color.FromArgb("#FF8080"),
                    _ => Colors.Transparent,
                };
            }

            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (color == Color.FromArgb("#dcfce8")) return (int)ScheduleStatus.Confirmed;
                if (color == Colors.LightGoldenrodYellow) return (int)ScheduleStatus.Pending;
                if (color == Colors.CornflowerBlue) return (int)ScheduleStatus.Concluded;
                if (color == Color.FromArgb("#FF8080")) return (int)ScheduleStatus.Canceled;
            }
            return (int)ScheduleStatus.Pending;
        }

    }
}
