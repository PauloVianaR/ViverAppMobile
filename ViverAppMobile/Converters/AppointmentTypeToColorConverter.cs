using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class AppointmentTypeToColorConverter : IValueConverter
    {
        public Color ConsultationColor { get; set; } = Color.FromArgb("#00c951");
        public Color ExaminationColor { get; set; } = Color.FromArgb("#2b7fff");
        public Color SurgeryColor { get; set; } = Color.FromArgb("#fb2c36");

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int aptype)
                return (Models.AppointmentType)aptype switch
                {
                    Models.AppointmentType.Consultation => ConsultationColor,
                    Models.AppointmentType.Examination => ExaminationColor,
                    Models.AppointmentType.Surgery => SurgeryColor,
                    _ => ConsultationColor
                };

            return ConsultationColor;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (color == ConsultationColor) return (int)Models.AppointmentType.Consultation;
                if (color == ExaminationColor) return (int)Models.AppointmentType.Examination;
                if (color == SurgeryColor) return (int)Models.AppointmentType.Surgery;
            }
            return (int)Models.AppointmentType.Consultation;
        }

    }
}
