using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public sealed partial class UserEditViewModel : ViewModelBase
{
    public User SourceItem
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
    }

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
    private bool _isChangingRole;

    private readonly PageController<User> _controller;
    [ObservableProperty] private bool _isEdit;

    private bool CanSaveChanges => IsEdit && !EditItem.PropertiesEqual(GlobalEditItem ?? SourceItem);
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;
    private bool CanResetEmail => IsEdit && EditItem.Email != SourceItem.Email;
    private bool CanResetRole => IsEdit && EditItem.Role != SourceItem.Role;


    public UserEditViewModel(PageController<User> controller, User sourceItem)
    {
        SourceItem = sourceItem;
        _controller = controller;
    }

    private async Task ChangeRole()
    {
        if (_isChangingRole) return;
        _isChangingRole = true;

        Role? role = Roles.FirstOrDefault(x => x.Id == SourceItem.Role.Id);
        if (role is null)
        {
            Roles = await FactoryProvider.Instance
                .GetHelperFactory<Role>()
                .TryGetAllFromCache(SourceItem.Id);
            role = Roles.First(x => x.Id == SourceItem.Role.Id);
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

        _controller.NavigateBack();
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
            Changes.WriteEdit<User>(EditItem.Copy());
        }

        _controller.NavigateBack();
    }
}