using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public sealed partial class UserEditViewModel(PageController<User> controller) : ItemEditViewModel<User>(controller)
{
    public override User SourceItem
    {
        get;
        set
        {
            field = value;
            _ = ChangeRole();
            EditItem?.PropertyChanged -= EditItemOnPropertyChanged;
            GlobalEditItem = EditableUser.TryGetFrom(value);
            EditItem!.PropertyChanged += EditItemOnPropertyChanged;
        }
    } = null!;

    private EditableUser? GlobalEditItem
    {
        get;
        set
        {
            field = value;
            if (value is null) EditItem = EditableUser.FromModel(SourceItem);
            else EditItem = value.Copy();
        }
    }

    [ObservableProperty] private EditableUser _editItem = null!;

    [ObservableProperty] private Role[] _roles = [];
    private DateTime _lastCacheCheck = DateTime.MinValue;
    private bool _isChangingRole;

    private bool CanSaveChanges => IsEdit && !EditItem.PropertiesEqual(GlobalEditItem ?? SourceItem);
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;
    private bool CanResetEmail => IsEdit && EditItem.Email != SourceItem.Email;
    private bool CanResetRole => IsEdit && EditItem.Role != SourceItem.Role;

    public override void OnOpening() => _ = ChangeRole();

    private async Task ChangeRole()
    {
        if (_isChangingRole) return;
        _isChangingRole = true;

        Role? role = Roles.FirstOrDefault();
        var factory = FactoryProvider.Instance.GetHelperFactory<Role>();

        if (role is null)
        {
            Roles = await factory.TryGetAllFromCache();
            role = Roles.First();
        }
        else if (factory.CacheUpdated > _lastCacheCheck)
        {
            _lastCacheCheck = factory.CacheUpdated;
            Roles = await factory.TryGetAllFromCache();
            role = Roles.First();
        }

        EditItem.Role = role;
        _isChangingRole = false;
    }

    private void EditItemOnPropertyChanged(object? sender, PropertyChangedEventArgs? e) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute()
    {
        ResetNameCommand.NotifyCanExecuteChanged();
        ResetEmailCommand.NotifyCanExecuteChanged();
        ResetRoleCommand.NotifyCanExecuteChanged();
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanResetName))]
    private void ResetName() => EditItem.Name = SourceItem.Name;

    [RelayCommand(CanExecute = nameof(CanResetEmail))]
    private void ResetEmail() => EditItem.Email = SourceItem.Email;

    [RelayCommand(CanExecute = nameof(CanResetRole))]
    private void ResetRole() => EditItem.Role = Roles.First(x => x.Id == SourceItem.Role.Id);

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
            if (CanResetEmail) ResetEmail();
            if (CanResetRole) ResetRole();
        }

        Controller.ClosePopup();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (GlobalEditItem is not null)
        {
            GlobalEditItem.SetPropertiesFrom(EditItem);

            if (GlobalEditItem.PropertiesEqual(SourceItem)) Changes.RemoveEdit<User>(GlobalEditItem);
        }
        else
        {
            Changes.Edit<User>(EditItem.Copy());
        }

        Controller.ClosePopup();
    }
}