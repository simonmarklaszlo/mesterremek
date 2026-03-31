using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Generated;
using SzivarClubManager.ViewModels.Activities;
using SzivarClubManager.ViewModels.AppState;

namespace SzivarClubManager.ViewModels;

public partial class MainContainerViewModel : ViewModelBase
{
    private readonly IReadOnlyDictionary<string, ActivityViewModel> _activities;
    [ObservableProperty] private ViewModelBase _currentActivity;
    [ObservableProperty] private string? _selectedActivityName;
    public string[] ActivityNames { get; }

    public MainContainerViewModel(FactoryProvider provider, PopupService popupService)
    {
        _activities = ActivityCollection.GetActivities(provider, popupService);

        _currentActivity = new LoadingViewModel();
        ActivityNames = _activities.Keys.ToArray();
        if (ActivityNames.Length > 0)
        {
            SelectedActivityName = ActivityNames[0];
        }
        else
        {
            _currentActivity = new NoActivitiesViewModel();
        }
    }

    partial void OnSelectedActivityNameChanged(string? value)
    {
        if (value is not null) _ = ChangeActivity(value);
    }

    [RelayCommand]
    private async Task ChangeActivity(string activityName)
    {
        var nextActivity = _activities[activityName];
        await nextActivity.InitializeAsync();
        CurrentActivity = nextActivity;
    }
}