using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SzivarClubManager.ViewModels.Activities.Page.Filter;

namespace SzivarClubManager.Converters;

public class FilterFieldStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FieldState state) return null;

        return state switch
        {
            FieldState.None => BindingOperations.DoNothing, // keep default border
            FieldState.Invalid => Brushes.Red,
            FieldState.Warning => Brushes.Orange,
            FieldState.Valid => Brushes.Green,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}