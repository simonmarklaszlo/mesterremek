using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.SourceGeneration.Generated;
using SzivarClubManager.ViewModels.Activities;

namespace SzivarClubManager.ViewModels;

public partial class MainContainerViewModel : ViewModelBase
{
    private readonly IReadOnlyDictionary<string, ActivityViewModel> _activities;
    [ObservableProperty] private ActivityViewModel _currentActivity;
    [ObservableProperty] private string? _selectedActivityName;
    public string[] ActivityNames { get; }

    public MainContainerViewModel(DatabaseConnection connection)
    {
        _activities = ActivityCollection.GetActivities(
            // FactoryProvider.Create(connection)
            FactoryProvider.Mock()
        );

        _currentActivity = new NoActivityViewModel();
        ActivityNames = _activities.Keys.ToArray();
        if (ActivityNames.Length > 0)
        {
            SelectedActivityName = ActivityNames[0];
        }
    }

    partial void OnSelectedActivityNameChanged(string? value)
    {
        if (value is not null) _ = ChangeActivity(value);
    }

    [RelayCommand]
    private async Task ChangeActivity(string activityName)
    {
        await _activities[activityName].InitializeAsync();
        CurrentActivity = _activities[activityName];
    }
}