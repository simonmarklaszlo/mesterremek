using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace SzivarClubManager.Behaviors;

public sealed class DataGridCurrentColumnBehavior : Behavior<DataGrid>
{
    public static readonly StyledProperty<DataGridColumn?> CurrentColumnProperty = AvaloniaProperty.Register<DataGridCurrentColumnBehavior, DataGridColumn?>(nameof(CurrentColumn));

    public DataGridColumn? CurrentColumn
    {
        get => GetValue(CurrentColumnProperty);
        set => SetValue(CurrentColumnProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject!.CurrentCellChanged += OnCurrentCellChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.CurrentCellChanged -= OnCurrentCellChanged;
        base.OnDetaching();
    }

    private void OnCurrentCellChanged(object? sender, EventArgs e)
    {
        CurrentColumn = AssociatedObject?.CurrentColumn;
    }
}