using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.Views.Shared;

public class EditField : ContentControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<EditField, string>(nameof(Label));
    public static readonly StyledProperty<bool> IsEditableProperty = AvaloniaProperty.Register<EditField, bool>(nameof(IsEditable), defaultValue: true);
    public static readonly StyledProperty<IRelayCommand> ResetCommandProperty = AvaloniaProperty.Register<EditField, IRelayCommand>(nameof(ResetCommand));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsEditable
    {
        get => GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    public IRelayCommand ResetCommand
    {
        get => GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }
}