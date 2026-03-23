using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System.Windows.Input;

namespace SzivarClubManager.Behaviors;

public class InfiniteScrollBehavior : Behavior<ScrollViewer>
{
    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty = AvaloniaProperty.Register<InfiniteScrollBehavior, ICommand?>(nameof(LoadMoreCommand));

    public ICommand? LoadMoreCommand
    {
        get => GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject!.ScrollChanged += OnScrollChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.ScrollChanged -= OnScrollChanged;
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var sv = AssociatedObject;

        if (sv!.Offset.Y >= sv.Extent.Height - sv.Viewport.Height - 100)
        {
            if (LoadMoreCommand?.CanExecute(null) == true)
                LoadMoreCommand.Execute(null);
        }
    }
}