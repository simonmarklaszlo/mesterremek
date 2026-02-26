using System.Threading.Tasks;

namespace SzivarClubManager.ViewModels.Activities;

public abstract class ActivityViewModel : ViewModelBase
{
    public bool IsInitialized { get; private set; }

    public virtual Task InitializeAsync()
    {
        if (IsInitialized) return Task.CompletedTask;
        IsInitialized = true;
        return Task.CompletedTask;
    }

    public virtual void OnOpening() { }
}