using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;
using SzivarClubManager.SourceGeneration.Generated;
using SzivarClubManager.ViewModels.Activities;
using SzivarClubManager.ViewModels.Activities.Change;

namespace SzivarClubManager.ViewModels;

public partial class MainContainerViewModel : ViewModelBase
{
    private readonly IReadOnlyDictionary<string, ActivityViewModel> _activities;
    [ObservableProperty] private ActivityViewModel _currentActivity;
    [ObservableProperty] private string? _selectedActivityName;
    public string[] ActivityNames { get; }

    public MainContainerViewModel(FactoryProvider provider)
    {
        _activities = ActivityCollection.GetActivities(provider)
            .Append(new KeyValuePair<string, ActivityViewModel>("Changes", new ChangesActivityViewModel()))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

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
        var nextActivity = _activities[activityName];
        await nextActivity.InitializeAsync();
        nextActivity.OnOpening();
        CurrentActivity = nextActivity;
    }
}