using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Converters
{
    public class ScheduleStatusToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intstatus)
            {
                var status = (ScheduleStatus)intstatus;

                return status switch
                {
                    ScheduleStatus.Confirmed => "confirmado",
                    ScheduleStatus.Pending => "pendente",
                    ScheduleStatus.Concluded => "realizado",
                    ScheduleStatus.Canceled => "cancelado",
                    ScheduleStatus.Rescheduled => "reagendado",
                    _ => status.ToString(),
                };
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                int aptype = parameter is int p ? p : (int)Models.AppointmentType.Consultation;

                return text switch
                {
                    "confirmado" => (int)ScheduleStatus.Confirmed,
                    "pendente" => (int)ScheduleStatus.Pending,
                    "realizado" => (int)ScheduleStatus.Concluded,
                    "cancelado" => (int)ScheduleStatus.Canceled,
                    "reagendado" => (int)ScheduleStatus.Rescheduled,
                    _ => (int)ScheduleStatus.Pending
                };
            }
            return (int)ScheduleStatus.Pending;
        }

    }
}
