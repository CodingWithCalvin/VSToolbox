using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace CodingWithCalvin.VSToolbox.Converters;

public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int count)
        {
            // Show if there are any hives detected
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
