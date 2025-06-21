using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MusicService.Domain.Entities;

namespace MusicService.WPF.Infrastructure;

public class RoleNameVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string role && parameter is Role roleEnumValue && role == roleEnumValue.ToString())
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}