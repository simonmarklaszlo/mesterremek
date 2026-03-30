using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

public abstract partial class FilterViewModel<TFilter, TModel> : ViewModelBase
    where TModel : class, IModel
    where TFilter : IFilter<TModel>
{
    public TFilter Filter { get; }
    protected PopupService PopupService { get; }
    protected IRelayCommand AfterApplyCommand { get; }

    protected FilterViewModel(PopupService popupService, TFilter filter, IRelayCommand afterApplyCommand)
    {
        PopupService = popupService;
        Filter = filter;
        AfterApplyCommand = afterApplyCommand;
    }

    [RelayCommand]
    private void Cancel()
    {
        Debugger.Break();
        PopupService.ClosePopup();
    }

    public abstract void RefreshFilterFields();
}