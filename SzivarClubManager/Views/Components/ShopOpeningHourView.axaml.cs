using Avalonia;
using Avalonia.Controls;
using SzivarClubManager.Models.Time;

namespace SzivarClubManager.Views.Components;

public partial class ShopOpeningHourView : UserControl
{
    public static readonly StyledProperty<ShopOpeningHour> ShopOpeningHourProperty = AvaloniaProperty.Register<ShopOpeningHourView, ShopOpeningHour>(nameof(ShopOpeningHour));

    public ShopOpeningHour ShopOpeningHour
    {
        get => GetValue(ShopOpeningHourProperty);
        set => SetValue(ShopOpeningHourProperty, value);
    }

    public ShopOpeningHourView()
    {
        InitializeComponent();
    }
}