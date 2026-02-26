using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SzivarClubManager.Datasources.Change;

namespace SzivarClubManager.Models.Editable;

public sealed class EditableShop : Shop, IEditableModel, INotifyPropertyChanged
{
    public new string Name
    {
        get => base.Name;
        set
        {
            base.Name = value;
            OnPropertyChanged();
        }
    }

    public new string Address
    {
        get => base.Address;
        set
        {
            base.Address = value;
            OnPropertyChanged();
        }
    }

    public new string City
    {
        get => base.City;
        set
        {
            base.City = value;
            OnPropertyChanged();
        }
    }

    public new CustomPgPoint Location
    {
        get => base.Location;
        set
        {
            base.Location = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;


    private EditableShop(int id, string name, string address, string city, DateTime createdAt, DateTime updatedAt, CustomPgPoint location) :
        base(id, name, address, city, createdAt, updatedAt, location) { }


    /*
    public bool EqualsImmutable(IModel other) => other is Shop shop && EqualsImmutable(shop);

    public bool EqualsImmutable(Shop other) => Id == other.Id &&
                                               Name == other.Name &&
                                               Address == other.Address &&
                                               City == other.City &&
                                               Location == other.Location;

    public bool EqualsImmutableExceptLocation(Shop other) => Id == other.Id &&
                                                             Name == other.Name &&
                                                             Address == other.Address &&
                                                             City == other.City;
                                                             */


    public void SetPropertiesFrom(Shop model)
    {
        Name = model.Name;
        Address = model.Address;
        City = model.City;
        Location = model.Location;
    }

    public EditableShop Copy() => new(Id, Name, Address, City, CreatedAt, UpdatedAt, Location.Copy());
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public bool PropertiesEqualExceptLocation(Shop model) => Id == model.Id && Name == model.Name && Address == model.Address && City == model.City;

    public bool PropertiesEqualWithStringLocationCompare(Shop model) => Id == model.Id &&
                                                                        Name == model.Name &&
                                                                        Address == model.Address &&
                                                                        City == model.City &&
                                                                        Location.LongitudeString == model.Location.LongitudeString &&
                                                                        Location.LatitudeString == model.Location.LatitudeString;

    public static EditableShop FromModel(Shop model) => new(model.Id, model.Name, model.Address, model.City, model.CreatedAt, model.UpdatedAt, model.Location);
    public static EditableShop? TryGetFrom(Shop model) => Changes.GetEdited<Shop>(model.Id) as EditableShop;
}