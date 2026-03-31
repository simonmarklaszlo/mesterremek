using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.AppState;

namespace SzivarClubManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentViewModel;
    [ObservableProperty] private ViewModelBase? _popupViewModel;
    [ObservableProperty] private bool _isPopupVisible;
    [ObservableProperty] private bool _isPopupHidden = true;

    private readonly PopupService _popupService;
    public IRelayCommand ClosePopupCommand { get; }

    public MainWindowViewModel()
    {
        _currentViewModel = new LoadingViewModel();
        _popupService = new PopupService(this);
        ClosePopupCommand = new RelayCommand(_popupService.ClosePopup);
        _ = InitAsync();
    }

    partial void OnPopupViewModelChanged(ViewModelBase? value)
    {
        IsPopupVisible = value is not null;
        IsPopupHidden = value is null;
    }

    private async Task InitAsync()
    {
        try
        {
            DatabaseConnection? connection = await DatabaseConnection.ConnectAsync();

            if (connection is null) CurrentViewModel = new ErrorViewModel("Sikertelen kapcsolódás adatbázishoz");
            else
            {
                FactoryProvider.CreateDatabase(connection);

                Changes.Initialize(FactoryProvider.Instance);

                CurrentViewModel = new MainContainerViewModel(FactoryProvider.Instance, _popupService);
            }
        }
        catch (Exception e)
        {
            CurrentViewModel = new ErrorViewModel(e.Message);
            Console.WriteLine(e);

            await Task.Delay(2000);
            Environment.Exit(-1);
        }
    }
}