using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public sealed partial class CigarAddViewModel : ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private Brand? _brand;

    private readonly PageController<Cigar> _controller;

    [ObservableProperty] private Brand[] _brands = [];


    private bool CanAdd => !string.IsNullOrWhiteSpace(Name) && Brand is not null;
    public CigarAddViewModel(PageController<Cigar> controller)
    {
        _ = ChangeBrand();
        _controller = controller;
    }


    private async Task ChangeBrand()
    {
        Brand? brand = Brands.FirstOrDefault();
        if (brand is null)
        {
            Brands = await FactoryProvider.Instance
                .GetHelperFactory<Brand>()
                .TryGetAllFromCache();
            brand = Brands.First();
        }

        Brand = brand;
    }

    partial void OnNameChanged(string value) => ReCheckCommandCanExecute();
    partial void OnBrandChanged(Brand? value) => ReCheckCommandCanExecute();

    private void ReCheckCommandCanExecute() => AddCommand.NotifyCanExecuteChanged();


    [RelayCommand]
    private void Cancel()
    {
        Name = string.Empty;
        Brand = Brands.First();

        _controller.NavigateBack();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Cigar newItem = Cigar.CreateNew(Name, Brand!);
        Changes.AddNew(newItem);
        _controller.NavigateBack();
    }
}