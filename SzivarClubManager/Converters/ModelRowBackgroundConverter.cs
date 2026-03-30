using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using SzivarClubManager.Models;

namespace SzivarClubManager.Converters;

public sealed partial class ModelRowBackgroundConverter : IMultiValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is IModel model ? GetBrush(model) : null;
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) => values[0] is IModel model ? GetBrush(model) : null;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    private static partial IBrush? GetBrush(IModel model);

    private static bool IsDarkTheme() => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    private static IImmutableSolidColorBrush GetEditBrush() => IsDarkTheme() ? Brushes.DarkSlateBlue : Brushes.LightBlue;
    private static IImmutableSolidColorBrush GetDeleteBrush() => IsDarkTheme() ? Brushes.DarkRed : Brushes.IndianRed;
}