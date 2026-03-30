using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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


    private readonly Func<Task> _suggestionApprovedCallback;


    public SuggestionWrapperViewModel(Suggestion suggestion, Func<Task> suggestionApprovedCallback)
    {
        Suggestion = suggestion;
        Content = GetSuggestionContent(suggestion);

        _isExpanded = false;
        IsToggleable = suggestion switch
        {
            EditShopAddressSuggestion or EditShopNameSuggestion => false,
            _ => true
        };

        _suggestionApprovedCallback = suggestionApprovedCallback;
    }

    [RelayCommand(CanExecute = nameof(CanBeApproved))]
    private Task ApproveSuggestion()
    {
        _suggestionApprovedCallback.Invoke();
        return Suggestion.Approve();
    }

    private static ViewModelBase GetSuggestionContent(Suggestion suggestion) => suggestion switch
    {
        EditShopAddressSuggestion esas => new EditShopAddressSuggestionViewModel(esas),
        EditShopNameSuggestion esns => new EditShopNameSuggestionViewModel(esns),
        EditShopOpeningHours esos => new EditShopOpeningHoursViewModel(esos),
        NewShopSuggestion nss => new NewShopSuggestionViewModel(nss),
        _ => throw new ArgumentOutOfRangeException(nameof(suggestion))
    };
}