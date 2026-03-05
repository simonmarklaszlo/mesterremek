using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SzivarClubManager.Datasources.Change;

namespace SzivarClubManager.Models.Editable;

public sealed class EditableUser : User, IEditableModel, INotifyPropertyChanged
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

    public new string Email
    {
        get => base.Email;
        set
        {
            base.Email = value;
            OnPropertyChanged();
        }
    }

    public new Role Role
    {
        get => base.Role;
        set
        {
            base.Role = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;


    private EditableUser(int id, string name, string email, DateTime createdAt, Role role) : base(id, name, email, createdAt, role) { }


    public void SetPropertiesFrom(User model)
    {
        Name = model.Name;
        Email = model.Email;
        Role = model.Role;
    }

    public EditableUser Copy() => new(Id, Name, Email, CreatedAt, Role);
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public bool PropertiesEqual(User model) => Id == model.Id && Name == model.Name && Email == model.Email && CreatedAt == model.CreatedAt && Role == model.Role;


    public static EditableUser FromModel(User model) => new(model.Id, model.Name, model.Email, model.CreatedAt, model.Role);
    public static EditableUser? TryGetFrom(User model) => Changes.GetEdited<User>(model.Id) as EditableUser;
}