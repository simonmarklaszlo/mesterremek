using System;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.Navigation;

public class PageController<T> where T : IModel
{
    public event Action<T, bool>? ItemSelected;
    public event Action? BackNavigated;

    public void SelectItem(T item, bool isEdit) => ItemSelected?.Invoke(item, isEdit);
    public void NavigateBack() => BackNavigated?.Invoke();
}