using Avalonia;
using Avalonia.Controls;

namespace SzivarClubManager.Views.Shared;

public class EditField : ContentControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<EditField, string>(nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}