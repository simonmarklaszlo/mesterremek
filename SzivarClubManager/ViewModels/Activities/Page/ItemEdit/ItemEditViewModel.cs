using SzivarClubManager.Models;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public abstract class ItemEditViewModel<T> : ViewModelBase where T : class, IModel
{
    public abstract T SourceItem { get; set; }
    protected PageController<T> Controller { get; }

    public bool IsEdit
    {
        get;
        set => SetProperty(ref field, value);
    }

    protected ItemEditViewModel(PageController<T> controller)
    {
        Controller = controller;
    }
}