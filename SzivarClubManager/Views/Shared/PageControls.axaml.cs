using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.Views.Shared;

public partial class PageControls : UserControl
{
    public static readonly StyledProperty<IRelayCommand> LoadFirstPageCommandProperty = AvaloniaProperty.Register<PageControls, IRelayCommand>(nameof(LoadFirstPageCommand));
    public static readonly StyledProperty<IRelayCommand> LoadPreviousPageCommandProperty = AvaloniaProperty.Register<PageControls, IRelayCommand>(nameof(LoadPreviousPageCommand));
    public static readonly StyledProperty<IRelayCommand> LoadNextPageCommandProperty = AvaloniaProperty.Register<PageControls, IRelayCommand>(nameof(LoadNextPageCommand));
    public static readonly StyledProperty<IRelayCommand> LoadLastPageCommandProperty = AvaloniaProperty.Register<PageControls, IRelayCommand>(nameof(LoadLastPageCommand));
    public static readonly StyledProperty<int> CurrentPageNumberProperty = AvaloniaProperty.Register<PageControls, int>(nameof(CurrentPageNumber));


    public IRelayCommand LoadFirstPageCommand
    {
        get => GetValue(LoadFirstPageCommandProperty);
        set => SetValue(LoadFirstPageCommandProperty, value);
    }

    public IRelayCommand LoadPreviousPageCommand
    {
        get => GetValue(LoadPreviousPageCommandProperty);
        set => SetValue(LoadPreviousPageCommandProperty, value);
    }


    public IRelayCommand LoadNextPageCommand
    {
        get => GetValue(LoadNextPageCommandProperty);
        set => SetValue(LoadNextPageCommandProperty, value);
    }

    public IRelayCommand LoadLastPageCommand
    {
        get => GetValue(LoadLastPageCommandProperty);
        set => SetValue(LoadLastPageCommandProperty, value);
    }

    public int CurrentPageNumber
    {
        get => GetValue(CurrentPageNumberProperty);
        set => SetValue(CurrentPageNumberProperty, value);
    }


    public PageControls()
    {
        InitializeComponent();
    }
}