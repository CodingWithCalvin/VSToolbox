using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace CodingWithCalvin.VSToolbox.Converters;

public sealed class ChannelTypeToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var channelType = value as string ?? "Unknown";

        return channelType switch
        {
            "Stable" => new SolidColorBrush(ColorHelper.FromArgb(255, 34, 197, 94)),    // Green
            "Preview" => new SolidColorBrush(ColorHelper.FromArgb(255, 168, 85, 247)),  // Purple
            "Canary" => new SolidColorBrush(ColorHelper.FromArgb(255, 251, 191, 36)),   // Amber/Yellow
            "Internal Preview" => new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)), // Red
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 107, 114, 128))          // Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
