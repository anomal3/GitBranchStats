using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitBranchStats.UI.Converters
{
    /// <summary>
    /// Converts integer stat values to color coding.
    /// High values = green, medium = yellow, low = gray.
    /// </summary>
    public class StatsToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                if (number > 100)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E7D32"));
                if (number > 20)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF9A825"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9E9E9E"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9E9E9E"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
