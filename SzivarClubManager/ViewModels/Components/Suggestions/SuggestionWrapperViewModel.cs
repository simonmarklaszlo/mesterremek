using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Factories;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.ViewModels.Components.Suggestions.SuggestionContent;

namespace SzivarClubManager.ViewModels.Components.Suggestions;

public sealed partial class SuggestionWrapperViewModel : ViewModelBase
{
    public Suggestion Suggestion { get; }
    [ObservableProperty] private bool _isExpanded;
    public ViewModelBase Content { get; }

    public bool IsToggleable { get; }
    public bool NotToggleable => !IsToggleable;
    public bool CanBeApproved => Suggestion.CanBeApproved;
    public bool CanBeDenied => Suggestion.CanBeDenied;


    private readonly Func<Task> _suggestionEditedCallback;


    public SuggestionWrapperViewModel(Suggestion suggestion, Func<Task> suggestionEditedCallback)
    {
        Suggestion = suggestion;
        Content = GetSuggestionContent(suggestion);

        _isExpanded = false;
        IsToggleable = suggestion switch
        {
            EditShopAddressSuggestion or EditShopNameSuggestion => false,
            _ => true
        };

        _suggestionEditedCallback = suggestionEditedCallback;
    }

    [RelayCommand(CanExecute = nameof(CanBeApproved))]
    private async Task ApproveSuggestion()
    {
        await SuggestionFactory.ApproveSuggestion(Suggestion);
        _ = _suggestionEditedCallback.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanBeDenied))]
    private async Task DenySuggestion()
    {
        await SuggestionFactory.DenySuggestion(Suggestion);
        _ = _suggestionEditedCallback.Invoke();
    }

    private static SuggestionFactory SuggestionFactory => field ??= (SuggestionFactory)FactoryProvider.Instance.GetFactory<Suggestion>();

    private static ViewModelBase GetSuggestionContent(Suggestion suggestion) => suggestion switch
    {
        EditShopAddressSuggestion esas => new EditShopAddressSuggestionViewModel(esas),
        EditShopNameSuggestion esns => new EditShopNameSuggestionViewModel(esns),
        EditShopOpeningHours esos => new EditShopOpeningHoursViewModel(esos),
        NewShopSuggestion nss => new NewShopSuggestionViewModel(nss),
        _ => throw new ArgumentOutOfRangeException(nameof(suggestion))
    };
}