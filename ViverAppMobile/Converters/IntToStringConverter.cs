using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class IntToStringConverter : IValueConverter
    {
        public string TextFor0 { get; set; } = string.Empty;
        public string TextFor1 { get; set; } = string.Empty;
        public string TextFor2 { get; set; } = string.Empty;
        public string TextFor3 { get; set; } = string.Empty;
        public string TextFor4 { get; set; } = string.Empty;
        public string TextFor5 { get; set; } = string.Empty;
        public string TextFor6 { get; set; } = string.Empty;
        public string TextFor7 { get; set; } = string.Empty;
        public string TextFor8 { get; set; } = string.Empty;
        public string TextFor9 { get; set; } = string.Empty;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is int index)
            {
                return index switch
                {
                    0 => TextFor0,
                    1 => TextFor1,
                    2 => TextFor2,
                    3 => TextFor3,
                    4 => TextFor4,
                    5 => TextFor5,
                    6 => TextFor6,
                    7 => TextFor7,
                    8 => TextFor8,
                    9 => TextFor9,
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
