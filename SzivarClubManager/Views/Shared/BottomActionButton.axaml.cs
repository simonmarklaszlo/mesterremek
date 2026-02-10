using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.Views.Shared;

public class BottomActionButton : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<BottomActionButton, string>(nameof(Text));
    public static readonly StyledProperty<IRelayCommand?> CommandProperty = AvaloniaProperty.Register<BottomActionButton, IRelayCommand?>(nameof(Command));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IRelayCommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}