using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Datasources.Mock;
using SzivarClubManager.Models;
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
        IDataSource dataSource = new MockDataSource(); //new DatabaseDataSource(connection);
        _activities = ActivityCollection.GetActivities(dataSource);

        _currentActivity = new NoActivityViewModel();
        ActivityNames = _activities.Keys.ToArray();
        if (ActivityNames.Length > 0)
        {
            SelectedActivityName = ActivityNames[0];
            ChangeActivity(ActivityNames[0]);
        }
    }


    partial void OnSelectedActivityNameChanged(string? value)
    {
        if (value is not null) ChangeActivity(value);
    }

    [RelayCommand]
    private void ChangeActivity(string activityName) => CurrentActivity = _activities[activityName];
}