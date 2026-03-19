using CommunityToolkit.Mvvm.ComponentModel;

namespace SzivarClubManager.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public virtual void OnOpening() { }
}