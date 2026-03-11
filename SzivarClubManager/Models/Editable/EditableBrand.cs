using System.ComponentModel;
using System.Runtime.CompilerServices;
using SzivarClubManager.Datasources.Change;

namespace SzivarClubManager.Models.Editable;

public class EditableBrand : Brand, IEditableModel, INotifyPropertyChanged
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


    public event PropertyChangedEventHandler? PropertyChanged;


    private EditableBrand(int id, string name) : base(id, name) { }


    public void SetPropertiesFrom(Brand model)
    {
        Name = model.Name;
    }

    public EditableBrand Copy() => new(Id, Name);
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public bool PropertiesEqual(Brand model) => Id == model.Id && Name == model.Name;


    public static EditableBrand FromModel(Brand model) => new(model.Id, model.Name);
    public static EditableBrand? TryGetFrom(Brand model) => Changes.GetEdited<Brand>(model.Id) as EditableBrand;
}