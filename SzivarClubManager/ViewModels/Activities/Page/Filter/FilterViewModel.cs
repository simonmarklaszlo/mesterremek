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
    public abstract TFilter Filter { get; init; }
    public PopupService PopupService { get; set; } = null!;
    protected IRelayCommand AfterApplyCommand { get; }

    protected FilterViewModel(IRelayCommand afterApplyCommand)
    {
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