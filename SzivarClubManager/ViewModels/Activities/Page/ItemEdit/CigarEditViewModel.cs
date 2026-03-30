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

public sealed partial class CigarEditViewModel(PageController<Cigar> controller) : ItemEditViewModel<Cigar>(controller)
{
    public override Cigar SourceItem
    {
        get;
        set
        {
            field = value;
            _ = ChangeBrand();
            EditItem?.PropertyChanged -= EditItemOnPropertyChanged;
            GlobalEditItem = EditableCigar.TryGetFrom(value);
            EditItem!.PropertyChanged += EditItemOnPropertyChanged;
        }
    } = null!;

    private EditableCigar? GlobalEditItem
    {
        get;
        set
        {
            field = value;
            if (value is null) EditItem = EditableCigar.FromModel(SourceItem);
            else EditItem = value.Copy();
        }
    }

    [ObservableProperty] private EditableCigar _editItem = null!;

    [ObservableProperty] private Brand[] _brands = [];
    private DateTime _lastCacheCheck = DateTime.MinValue;
    private bool _isChangingBrand;

    private bool CanSaveChanges => IsEdit && !EditItem.PropertiesEqual(GlobalEditItem ?? SourceItem);
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;
    private bool CanResetBrand => IsEdit && EditItem.Brand != SourceItem.Brand;

    public override void OnOpening() => _ = ChangeBrand();

    private async Task ChangeBrand()
    {
        if (_isChangingBrand) return;
        _isChangingBrand = true;

        Brand? brand = Brands.FirstOrDefault();
        var factory = FactoryProvider.Instance.GetHelperFactory<Brand>();

        if (brand is null)
        {
            Brands = await factory.TryGetAllFromCache();
            brand = Brands.First();
        }
        else if (factory.CacheUpdated > _lastCacheCheck)
        {
            _lastCacheCheck = factory.CacheUpdated;
            Brands = await factory.TryGetAllFromCache();
            brand = Brands.First();
        }

        EditItem?.Brand = brand;
        _isChangingBrand = false;
    }

    private void EditItemOnPropertyChanged(object? sender, PropertyChangedEventArgs? e) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute()
    {
        ResetNameCommand.NotifyCanExecuteChanged();
        ResetBrandCommand.NotifyCanExecuteChanged();
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanResetName))]
    private void ResetName() => EditItem.Name = SourceItem.Name;

    [RelayCommand(CanExecute = nameof(CanResetBrand))]
    private void ResetBrand() => EditItem.Brand = Brands.First(x => x.Id == SourceItem.Brand.Id);

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
            if (CanResetBrand) ResetBrand();
        }

        Controller.ClosePopup();
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (GlobalEditItem is not null)
        {
            GlobalEditItem.SetPropertiesFrom(EditItem);

            if (GlobalEditItem.PropertiesEqual(SourceItem)) Changes.RemoveEdit<Cigar>(GlobalEditItem);
        }
        else
        {
            Changes.Edit<Cigar>(EditItem.Copy());
        }

        Controller.ClosePopup();
    }
}