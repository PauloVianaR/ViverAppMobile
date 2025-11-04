using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class DecimalToBrlMoneyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal price)
                return price.ToString("c2", new CultureInfo("pt-BR"));

            return "0.00";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (decimal.TryParse(text, NumberStyles.Currency, new CultureInfo("pt-BR"), out var result))
                    return result;
            }
            return 0m;
        }

    }
}
