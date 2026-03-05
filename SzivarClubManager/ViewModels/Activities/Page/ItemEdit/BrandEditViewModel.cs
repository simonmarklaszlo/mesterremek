using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Editable;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public sealed partial class BrandEditViewModel : ItemEditViewModel<Brand>
{
    public override Brand SourceItem
    {
        get;
        set
        {
            field = value;
            EditItem?.PropertyChanged -= EditItemOnPropertyChanged;
            GlobalEditItem = EditableBrand.TryGetFrom(value);
            EditItem!.PropertyChanged += EditItemOnPropertyChanged;
        }
    } = null!;

    private EditableBrand? GlobalEditItem
    {
        get;
        set
        {
            field = value;
            if (value is null) EditItem = EditableBrand.FromModel(SourceItem);
            else EditItem = value.Copy();
        }
    }

    [ObservableProperty] private EditableBrand _editItem = null!;

    private bool CanSaveChanges => IsEdit && !EditItem.PropertiesEqual(GlobalEditItem ?? SourceItem);
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;


    private void EditItemOnPropertyChanged(object? sender, PropertyChangedEventArgs? e) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute()
    {
        ResetNameCommand.NotifyCanExecuteChanged();
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanResetName))]
    private void ResetName() => EditItem.Name = SourceItem.Name;


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
        }

        Controller.NavigateBack();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (GlobalEditItem is not null)
        {
            GlobalEditItem.SetPropertiesFrom(EditItem);

            if (GlobalEditItem.PropertiesEqual(SourceItem)) Changes.RemoveEdit<Brand>(GlobalEditItem);
        }
        else
        {
            Changes.Edit<Brand>(EditItem.Copy());
        }

        Controller.NavigateBack();
    }}