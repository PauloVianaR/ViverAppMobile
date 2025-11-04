using System.Globalization;

namespace ViverAppMobile.Converters
{
    public class SelectedValueToThicknessConverter : IValueConverter
    {
        public Thickness SelectedThickness { get; set; } = new(0, 5, 0, 0);
        public Thickness UnselectedThickness { get; set; } = new Thickness(0, 10, 0, 0);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value.Equals(parameter))
                return SelectedThickness;

            return UnselectedThickness;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d == SelectedThickness) return parameter;
                if (d == UnselectedThickness) return null;
            }
            return null;
        }
    }
}
