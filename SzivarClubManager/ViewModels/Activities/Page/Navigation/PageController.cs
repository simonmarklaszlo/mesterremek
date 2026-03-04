using System;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.Navigation;

public class PageController<T> where T : IModel
{
    /*public event Action<T, bool>? ItemSelected;
    public event Action? BackNavigated;

    public void SelectItem(T item, bool isEdit) => ItemSelected?.Invoke(item, isEdit);
    public void NavigateBack() => BackNavigated?.Invoke();*/


    public event Action? ItemAdded;
    public event Action<T>? ItemSelected;
    public event Action<T>? ItemEdited;
    public event Action? BackNavigated;

    public void AddItem() => ItemAdded?.Invoke();
    public void SelectItem(T item) => ItemSelected?.Invoke(item);
    public void EditItem(T item) => ItemEdited?.Invoke(item);
    public void NavigateBack() => BackNavigated?.Invoke();
}