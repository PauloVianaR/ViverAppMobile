using System.Globalization;

namespace ViverAppMobile.Converters
{
    public class SelectedValueToColorConverter : IValueConverter
    {
        public Color SelectedColor { get; set; } = Color.FromArgb("#F0F6FF");
        public Color UnselectedColor { get; set; } = Colors.White;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.Equals(parameter))
                return SelectedColor;

            return UnselectedColor;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                if (color == SelectedColor) return parameter;
                if (color == UnselectedColor) return null;
            }
            return null;
        }

    }
}
