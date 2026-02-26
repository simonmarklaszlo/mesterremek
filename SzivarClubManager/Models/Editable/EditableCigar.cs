using System.ComponentModel;
using System.Runtime.CompilerServices;
using SzivarClubManager.Datasources.Change;

namespace SzivarClubManager.Models.Editable;

public sealed class EditableCigar : Cigar, IEditableModel, INotifyPropertyChanged
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

    public new Brand Brand
    {
        get => base.Brand;
        set
        {
            base.Brand = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;


    private EditableCigar(int id, string name, Brand brand) : base(id, name, brand) { }


    public void SetPropertiesFrom(Cigar model)
    {
        Name = model.Name;
        Brand = model.Brand;
    }

    public EditableCigar Copy() => new(Id, Name, Brand);
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public bool PropertiesEqual(Cigar model) => Id == model.Id && Name == model.Name && Brand == model.Brand;


    public static EditableCigar FromModel(Cigar model) => new(model.Id, model.Name, model.Brand);
    public static EditableCigar? TryGetFrom(Cigar model) => Changes.GetEdited<Cigar>(model.Id) as EditableCigar;
}