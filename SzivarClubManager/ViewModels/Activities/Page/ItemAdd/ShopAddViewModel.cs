using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public sealed partial class ShopAddViewModel : ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _longitudeString = string.Empty;
    [ObservableProperty] private string _latitudeString = string.Empty;


    private readonly PageController<Shop> _controller;


    private bool CanAdd => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Address) && !string.IsNullOrWhiteSpace(City) &&
                           double.TryParse(LongitudeString, out _) && double.TryParse(LatitudeString, out _);

    public ShopAddViewModel(PageController<Shop> controller)
    {
        _controller = controller;
    }


    partial void OnNameChanged(string value) => ReCheckCommandCanExecute();
    partial void OnAddressChanged(string value) => ReCheckCommandCanExecute();
    partial void OnCityChanged(string value) => ReCheckCommandCanExecute();
    partial void OnLongitudeStringChanged(string value) => ReCheckCommandCanExecute();
    partial void OnLatitudeStringChanged(string value) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute() => AddCommand.NotifyCanExecuteChanged();


    [RelayCommand]
    private void Cancel()
    {
        Name = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        LongitudeString = string.Empty;
        LatitudeString = string.Empty;

        _controller.NavigateBack();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Shop newItem = Shop.CreateNew(Name, Address, City, new CustomPgPoint(double.Parse(LongitudeString), double.Parse(LatitudeString)));
        Changes.AddNew(newItem);
        _controller.NavigateBack();
    }
}