using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Converters
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not float rating)
                return "[não avaliado ainda]";

            if(rating <= 0)
                return "[não avaliado ainda]";

            int intRating = Math.Clamp((int)rating, 0, 5);
            var filledStars = Enumerable.Repeat("\ue835", intRating);
            var emptyStars = Enumerable.Repeat("\ue834", 5 - intRating);
            var allStars = filledStars.Concat(emptyStars);

            return string.Join("  ", allStars);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str || string.IsNullOrWhiteSpace(str))
                return 0f;

            if (str.Trim().Equals("[não avaliado ainda]", StringComparison.OrdinalIgnoreCase))
                return 0f;

            int filledCount = str.Count(c => c == '\ue835');

            return (float)Math.Clamp(filledCount, 0, 5);
        }
    }
}
