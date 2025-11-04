using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{

    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not DateTime dateTime)
                return string.Empty;

            var diff = DateTime.Now - dateTime;

            if (diff.TotalDays >= 365)
                return $"{Math.Floor(diff.TotalDays / 365)}a atrás";
            if (diff.TotalDays >= 30)
                return $"{Math.Floor(diff.TotalDays / 30)}meses atrás";
            if (diff.TotalDays >= 1)
                return $"{Math.Floor(diff.TotalDays)}d atrás";
            if (diff.TotalHours >= 1)
                return $"{Math.Floor(diff.TotalHours)}h atrás";
            if (diff.TotalMinutes >= 1)
                return $"{Math.Floor(diff.TotalMinutes)}min atrás";

            return "agora mesmo";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string text)
                return DateTime.Now;

            text = text.Trim().ToLowerInvariant();

            if (text == "agora mesmo")
                return DateTime.Now;

            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1)
                return DateTime.Now;

            var token = parts[0];
            if (token.Length < 2)
                return DateTime.Now;

            if (!int.TryParse(token[..^1], out int valor))
                return DateTime.Now;

            char unidade = token[^1];

            return unidade switch
            {
                'm' => DateTime.Now.AddMinutes(-valor),
                'h' => DateTime.Now.AddHours(-valor),
                'd' => DateTime.Now.AddDays(-valor),
                'M' => DateTime.Now.AddMonths(-valor),
                'a' => DateTime.Now.AddYears(-valor),
                _ => DateTime.Now
            };
        }
    }
}
