using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace SzivarClubManager.Behaviors;

public sealed class DataGridSelectedItemsBehavior : Behavior<DataGrid>
{
    public static readonly StyledProperty<IList?> SelectedItemsProperty = AvaloniaProperty.Register<DataGridSelectedItemsBehavior, IList?>(nameof(SelectedItems));

    public IList? SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject!.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.SelectionChanged -= OnSelectionChanged;
        base.OnDetaching();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AssociatedObject?.SelectedItems != null)
        {
            SelectedItems = AssociatedObject.SelectedItems.Cast<object>().ToList();
        }
    }
}