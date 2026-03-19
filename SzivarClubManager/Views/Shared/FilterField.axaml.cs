using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.ViewModels.Activities.Page.Filter;

namespace SzivarClubManager.Views.Shared;

public partial class FilterField : UserControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<FilterField, string>(nameof(Label));
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<FilterField, string>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<FieldState> StateProperty = AvaloniaProperty.Register<FilterField, FieldState>(nameof(State));
    public static readonly StyledProperty<IRelayCommand> ClearCommandProperty = AvaloniaProperty.Register<FilterField, IRelayCommand>(nameof(ClearCommand));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public FieldState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public IRelayCommand ClearCommand
    {
        get => GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public FilterField()
    {
        InitializeComponent();
    }
}