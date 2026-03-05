using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemEdit;

public abstract class ItemEditViewModel<T> : ViewModelBase where T : class, IModel
{
    public abstract T SourceItem { get; set; }
    public PageController<T> Controller { get; init; } = null!;

    public bool IsEdit
    {
        get;
        set => SetProperty(ref field, value);
    }
}