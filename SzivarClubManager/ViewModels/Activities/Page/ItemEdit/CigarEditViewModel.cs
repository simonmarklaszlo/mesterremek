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

public sealed partial class CigarEditViewModel : ViewModelBase
{
    public Cigar SourceItem
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
    }

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
    private bool _isChangingBrand;

    private readonly PageController<Cigar> _controller;
    [ObservableProperty] private bool _isEdit;

    private bool CanSaveChanges => IsEdit && !EditItem.PropertiesEqual(GlobalEditItem ?? SourceItem);
    private bool CanResetName => IsEdit && EditItem.Name != SourceItem.Name;
    private bool CanResetBrand => IsEdit && EditItem.Brand != SourceItem.Brand;


    public CigarEditViewModel(PageController<Cigar> controller, Cigar sourceItem)
    {
        SourceItem = sourceItem;
        _controller = controller;
    }

    private async Task ChangeBrand()
    {
        if (_isChangingBrand) return;
        _isChangingBrand = true;

        Brand? brand = Brands.FirstOrDefault(x => x.Id == SourceItem.Brand.Id);
        if (brand is null)
        {
            Brands = await FactoryProvider.Instance
                .GetHelperFactory<Brand>()
                .TryGetAllFromCache(SourceItem.Id);
            brand = Brands.First(x => x.Id == SourceItem.Brand.Id);
        }

        EditItem.Brand = brand;
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

        _controller.NavigateBack();
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

        _controller.NavigateBack();
    }
}