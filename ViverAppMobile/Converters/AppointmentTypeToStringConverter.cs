using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class AppointmentTypeToStringConverter : IValueConverter
    {
        public string ConsultationText { get; set; } = "Consulta";
        public string ExaminationText { get; set; } = "Exame";
        public string SurgeryText { get; set; } = "Cirurgia";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int aptype)
                return (Models.AppointmentType)aptype switch
                {
                    Models.AppointmentType.Consultation => ConsultationText,
                    Models.AppointmentType.Examination => ExaminationText,
                    Models.AppointmentType.Surgery => SurgeryText,
                    _ => ConsultationText
                };

            return ConsultationText;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (text == ConsultationText) return (int)Models.AppointmentType.Consultation;
                if (text == ExaminationText) return (int)Models.AppointmentType.Examination;
                if (text == SurgeryText) return (int)Models.AppointmentType.Surgery;
            }
            return (int)Models.AppointmentType.Consultation;
        }

    }
}
