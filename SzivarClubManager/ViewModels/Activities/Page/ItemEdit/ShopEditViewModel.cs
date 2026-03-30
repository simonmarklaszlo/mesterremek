using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Components;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public sealed partial class ShopEditViewModel(PageController<Shop> controller) : ItemEditViewModel<Shop>(controller)
{
    public override Shop SourceItem
    {
        get;
        set
        {
            field = value;
            ScheduleViewModel.SetShop(value);
            EditItem?.PropertyChanged -= EditItemOnPropertyChanged;
            GlobalEditItem = EditableShop.TryGetFrom(value);
            LongitudeString = EditItem!.Location.LongitudeString;
            LatitudeString = EditItem.Location.LatitudeString;
            EditItem.PropertyChanged += EditItemOnPropertyChanged;
        }
    } = null!;

    private EditableShop? GlobalEditItem
    {
        get;
        set
        {
            field = value;
            if (value is null) EditItem = EditableShop.FromModel(SourceItem);
            else EditItem = value.Copy();
        }
    }

    [ObservableProperty] private EditableShop _editItem = null!;

    [ObservableProperty] private string _longitudeString = string.Empty;
    [ObservableProperty] private string _latitudeString = string.Empty;

    public ShopOpeningScheduleViewModel ScheduleViewModel { get; } = new();

    private bool CanSaveChanges => IsEdit && (!EditItem.PropertiesEqualExceptLocation(GlobalEditItem ?? SourceItem) || NewLocationValid());
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;
    private bool CanResetAddress => IsEdit && EditItem.Address != SourceItem.Address;
    private bool CanResetCity => IsEdit && EditItem.City != SourceItem.City;
    private bool CanResetLocation => IsEdit && SourceItem.Location.LongitudeString != LongitudeString || SourceItem.Location.LatitudeString != LatitudeString;


    private bool NewLocationValid()
    {
        if (string.IsNullOrWhiteSpace(LongitudeString) || string.IsNullOrWhiteSpace(LatitudeString)) return false;

        if (!double.TryParse(LongitudeString, out double _)) return false;
        if (!double.TryParse(LatitudeString, out double _)) return false;

        if (GlobalEditItem is not null)
        {
            return GlobalEditItem.Location.LongitudeString != LongitudeString ||
                   GlobalEditItem.Location.LatitudeString != LatitudeString;
        }

        return SourceItem.Location.LongitudeString != LongitudeString ||
               SourceItem.Location.LatitudeString != LatitudeString;
    }

    private void EditItemOnPropertyChanged(object? sender, PropertyChangedEventArgs? e) => ReCheckCommandCanExecute();
    partial void OnLatitudeStringChanged(string value) => ReCheckCommandCanExecute();
    partial void OnLongitudeStringChanged(string value) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute()
    {
        ResetNameCommand.NotifyCanExecuteChanged();
        ResetAddressCommand.NotifyCanExecuteChanged();
        ResetCityCommand.NotifyCanExecuteChanged();
        ResetLocationCommand.NotifyCanExecuteChanged();
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanResetName))]
    private void ResetName() => EditItem.Name = SourceItem.Name;

    [RelayCommand(CanExecute = nameof(CanResetAddress))]
    private void ResetAddress() => EditItem.Address = SourceItem.Address;

    [RelayCommand(CanExecute = nameof(CanResetCity))]
    private void ResetCity() => EditItem.City = SourceItem.City;

    [RelayCommand(CanExecute = nameof(CanResetLocation))]
    private void ResetLocation()
    {
        LongitudeString = SourceItem.Location.LongitudeString;
        LatitudeString = SourceItem.Location.LatitudeString;
    }

    [RelayCommand]
    private void DropChanges()
    {
        if (GlobalEditItem is not null)
        {
            EditItem.SetPropertiesFrom(GlobalEditItem);
        }
        else
        {
            if (CanResetName) ResetName();
            if (CanResetAddress) ResetAddress();
            if (CanResetCity) ResetCity();
            if (CanResetLocation) ResetLocation();
        }

        Controller.ClosePopup();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        EditItem.Location = new CustomPgPoint(double.Parse(LongitudeString), double.Parse(LatitudeString));

        if (GlobalEditItem is not null)
        {
            GlobalEditItem.SetPropertiesFrom(EditItem);

            if (GlobalEditItem.PropertiesEqualWithStringLocationCompare(SourceItem)) Changes.RemoveEdit<Shop>(GlobalEditItem);
        }
        else
        {
            Changes.Edit<Shop>(EditItem.Copy());
        }

        Controller.ClosePopup();
    }
}