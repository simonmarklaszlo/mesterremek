using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

public abstract partial class MinimalFilterViewModel<TFilter, TModel> : ViewModelBase, IMinimalFilterViewModel
    where TModel : class, IModel
    where TFilter : IFilter<TModel>
{
    [ObservableProperty] private string _searchText = string.Empty;
    public IRelayCommand ShowFilterCommand { get; }
    public IRelayCommand TriggerSearchCommand { get; }

    private bool CanClearFilter => SearchText != string.Empty;

    private readonly TFilter _filter;

    protected MinimalFilterViewModel(TFilter filter, IRelayCommand showFilterCommand, IRelayCommand triggerSearchCommand)
    {
        _filter = filter;
        ShowFilterCommand = showFilterCommand;
        TriggerSearchCommand = triggerSearchCommand;
    }

    [RelayCommand(CanExecute = nameof(CanClearFilter))]
    private void ClearFilter()
    {
        SearchText = string.Empty;
        TriggerSearchCommand.Execute(null);
    }

    partial void OnSearchTextChanged(string value)
    {
        ClearFilterCommand.NotifyCanExecuteChanged();
        TFilter.Parse(value).CopyTo(_filter);
    }
}