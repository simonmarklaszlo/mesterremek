using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public sealed partial class CigarAddViewModel : ItemAddViewModel
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private Brand? _brand;

    [ObservableProperty] private Brand[] _brands = [];
    private DateTime _lastCacheCheck = DateTime.MinValue;
    private bool _isChangingBrand;

    protected override bool CanAdd => !string.IsNullOrWhiteSpace(Name) && Brand is not null;

    public CigarAddViewModel(PopupService popupService) : base(popupService) => _ = ChangeBrand();

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

        Brand = brand;
        _isChangingBrand = false;
    }

    partial void OnNameChanged(string value) => ReCheckCommandCanExecute();
    partial void OnBrandChanged(Brand? value) => ReCheckCommandCanExecute();

    protected override void ResetFields()
    {
        Name = string.Empty;
        Brand = Brands.First();
    }

    protected override void AddNewItemToChanges()
    {
        Cigar newItem = Cigar.CreateNew(Name, Brand!);
        Changes.AddNew(newItem);
    }
}