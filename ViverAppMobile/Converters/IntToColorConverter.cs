using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        public Color ColorFor0 { get; set; } = Colors.Transparent;
        public Color ColorFor1 { get; set; } = Colors.Transparent;
        public Color ColorFor2 { get; set; } = Colors.Transparent;
        public Color ColorFor3 { get; set; } = Colors.Transparent;
        public Color ColorFor4 { get; set; } = Colors.Transparent;
        public Color ColorFor5 { get; set; } = Colors.Transparent;
        public Color ColorFor6 { get; set; } = Colors.Transparent;
        public Color ColorFor7 { get; set; } = Colors.Transparent;
        public Color ColorFor8 { get; set; } = Colors.Transparent;
        public Color ColorFor9 { get; set; } = Colors.Transparent;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is int index)
            {
                return index switch
                {
                    0 => ColorFor0,
                    1 => ColorFor1,
                    2 => ColorFor2,
                    3 => ColorFor3,
                    4 => ColorFor4,
                    5 => ColorFor5,
                    6 => ColorFor6,
                    7 => ColorFor7,
                    8 => ColorFor8,
                    9 => ColorFor9,
                    _ => Colors.Transparent
                };
            }

            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
