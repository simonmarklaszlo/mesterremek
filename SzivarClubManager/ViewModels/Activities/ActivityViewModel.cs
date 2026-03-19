using System.Threading.Tasks;
using SzivarClubManager.Services;

namespace SzivarClubManager.ViewModels.Activities;

public abstract class ActivityViewModel : ViewModelBase
{
    protected PopupService PopupService { get; }
    public bool IsInitialized { get; private set; }

    protected ActivityViewModel(PopupService popupService)
    {
        PopupService = popupService;
    }

    public virtual Task InitializeAsync()
    {
        if (IsInitialized) return Task.CompletedTask;
        IsInitialized = true;
        return Task.CompletedTask;
    }
}