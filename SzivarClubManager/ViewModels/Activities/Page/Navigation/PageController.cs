using System;

namespace SzivarClubManager.ViewModels.Activities.Page.Navigation;

public class PageController<T>
{
    public event Action<T, NavigationIntent>? ItemSelected;
    public event Action? BackNavigated;

    public void SelectItem(T item, NavigationIntent intent) => ItemSelected?.Invoke(item, intent);
    public void NavigateBack() => BackNavigated?.Invoke();
}