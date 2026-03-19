using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.ViewModels.Activities.Page.Filter;

namespace SzivarClubManager.Views.Shared;

public partial class FilterRangeField : UserControl
{
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<FilterRangeField, string>(nameof(Label));
    public static readonly StyledProperty<string> MinTextProperty = AvaloniaProperty.Register<FilterRangeField, string>(nameof(MinText), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<FieldState> MinStateProperty = AvaloniaProperty.Register<FilterRangeField, FieldState>(nameof(MinState));
    public static readonly StyledProperty<string> MaxTextProperty = AvaloniaProperty.Register<FilterRangeField, string>(nameof(MaxText), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<FieldState> MaxStateProperty = AvaloniaProperty.Register<FilterRangeField, FieldState>(nameof(MaxState));
    public static readonly StyledProperty<IRelayCommand> ClearCommandProperty = AvaloniaProperty.Register<FilterRangeField, IRelayCommand>(nameof(ClearCommand));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string MinText
    {
        get => GetValue(MinTextProperty);
        set => SetValue(MinTextProperty, value);
    }

    public FieldState MinState
    {
        get => GetValue(MinStateProperty);
        set => SetValue(MinStateProperty, value);
    }

    public string MaxText
    {
        get => GetValue(MaxTextProperty);
        set => SetValue(MaxTextProperty, value);
    }

    public FieldState MaxState
    {
        get => GetValue(MaxStateProperty);
        set => SetValue(MaxStateProperty, value);
    }

    public IRelayCommand ClearCommand
    {
        get => GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }


    public FilterRangeField()
    {
        InitializeComponent();
    }
}