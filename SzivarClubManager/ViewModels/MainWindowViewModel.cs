using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources.Database;

namespace SzivarClubManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase _currentViewModel;

    public MainWindowViewModel()
    {
        _currentViewModel = ConnectionStateViewModel.Loading;
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        DatabaseConnection? connection = await DatabaseConnection.ConnectAsync();

        Console.WriteLine("Connection to database : " + (connection is null ? "failure" : "succeess"));

        if (connection is null) CurrentViewModel = ConnectionStateViewModel.Error;
        else CurrentViewModel = new MainContainerViewModel(connection);
    }
}