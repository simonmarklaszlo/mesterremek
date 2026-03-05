using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public sealed partial class ShopAddViewModel : ItemAddViewModel<Shop>
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _longitudeString = string.Empty;
    [ObservableProperty] private string _latitudeString = string.Empty;


    protected override bool CanAdd => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Address) && !string.IsNullOrWhiteSpace(City) &&
                                      double.TryParse(LongitudeString, out _) && double.TryParse(LatitudeString, out _);



    partial void OnNameChanged(string value) => ReCheckCommandCanExecute();
    partial void OnAddressChanged(string value) => ReCheckCommandCanExecute();
    partial void OnCityChanged(string value) => ReCheckCommandCanExecute();
    partial void OnLongitudeStringChanged(string value) => ReCheckCommandCanExecute();
    partial void OnLatitudeStringChanged(string value) => ReCheckCommandCanExecute();


    protected override void ResetFields()
    {
        Name = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        LongitudeString = string.Empty;
        LatitudeString = string.Empty;
    }

    protected override void AddNewItemToChanges()
    {
        Shop newItem = Shop.CreateNew(Name, Address, City, new CustomPgPoint(double.Parse(LongitudeString), double.Parse(LatitudeString)));
        Changes.AddNew(newItem);
    }
}