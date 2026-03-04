using Avalonia;
using Avalonia.Controls;

namespace SzivarClubManager.Views.Shared;

public class AddField : ContentControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<AddField, string>(nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}