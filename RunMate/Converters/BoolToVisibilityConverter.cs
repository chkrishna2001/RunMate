using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RunMate.Converters;
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isVisible = false;
        var reverse = false;

        // Check if parameter requests reverse logic
        if (parameter != null && string.Equals(parameter.ToString(), "reverse", StringComparison.OrdinalIgnoreCase))
        {
            reverse = true;
        }

        // Handle boolean value
        if (value is bool boolValue)
        {
            isVisible = reverse ? !boolValue : boolValue;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var reverse = parameter?.ToString()?.Equals("reverse", StringComparison.OrdinalIgnoreCase) ?? false;
            return reverse ?
                (visibility != Visibility.Visible) :
                (visibility == Visibility.Visible);
        }
        return false;
    }
}
