using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.Views.Shared;

public partial class FilterView : UserControl
{
    public static readonly StyledProperty<IRelayCommand> CancelCommandProperty = AvaloniaProperty.Register<FilterView, IRelayCommand>(nameof(CancelCommand));
    public static readonly StyledProperty<IRelayCommand> ApplyCommandProperty = AvaloniaProperty.Register<FilterView, IRelayCommand>(nameof(ApplyCommand));
    public static readonly StyledProperty<object?> FilterContentProperty = AvaloniaProperty.Register<FilterView, object?>(nameof(FilterContent));

    public IRelayCommand CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public IRelayCommand ApplyCommand
    {
        get => GetValue(ApplyCommandProperty);
        set => SetValue(ApplyCommandProperty, value);
    }

    public object? FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }

    public ObservableCollection<object> FilterItems { get; } = [];


    public FilterView()
    {
        InitializeComponent();
    }
}