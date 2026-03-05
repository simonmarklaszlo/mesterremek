using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
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
        try
        {
            DatabaseConnection? connection = await DatabaseConnection.ConnectAsync();
            Console.WriteLine("Connection to database : " + (connection is null ? "failure" : "succeess"));

            if (connection is null) CurrentViewModel = ConnectionStateViewModel.Error;
            else
            {
                FactoryProvider.CreateDatabase(connection);

                Changes.Initialize(FactoryProvider.Instance);

                CurrentViewModel = new MainContainerViewModel(FactoryProvider.Instance);
            }
        }
        catch (Exception e)
        {
            CurrentViewModel = ConnectionStateViewModel.Error;
            Console.WriteLine(e);

            await Task.Delay(2000);
            Environment.Exit(-1);
        }
    }
}