using System.Globalization;

namespace ViverAppMobile.Converters
{
    public class IntToDayOfWeekConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intVal && Enum.IsDefined(typeof(DayOfWeek), intVal))
            {
                var day = (DayOfWeek)intVal;
                return day switch
                {
                    DayOfWeek.Sunday => "Domingo",
                    DayOfWeek.Monday => "Segunda-feira",
                    DayOfWeek.Tuesday => "Terça-feira",
                    DayOfWeek.Wednesday => "Quarta-feira",
                    DayOfWeek.Thursday => "Quinta-feira",
                    DayOfWeek.Friday => "Sexta-feira",
                    DayOfWeek.Saturday => "Sábado",
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strVal)
            {
                return strVal.ToLower(culture) switch
                {
                    "domingo" => (int)DayOfWeek.Sunday,
                    "segunda-feira" => (int)DayOfWeek.Monday,
                    "terça-feira" => (int)DayOfWeek.Tuesday,
                    "quarta-feira" => (int)DayOfWeek.Wednesday,
                    "quinta-feira" => (int)DayOfWeek.Thursday,
                    "sexta-feira" => (int)DayOfWeek.Friday,
                    "sábado" => (int)DayOfWeek.Saturday,
                    _ => Binding.DoNothing
                };
            }
            return Binding.DoNothing;
        }
    }
}
