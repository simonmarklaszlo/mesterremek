using System;
using SzivarClubManager.Models;

namespace SzivarClubManager.Services;

public class PageController<T> where T : class, IModel
{
    public event Action? AddPopupRequested;
    public event Action<T>? ViewPopupRequested;
    public event Action<T>? EditPopupRequested;
    public event Action? PopupCloseRequested;

    public void AddModel() => AddPopupRequested?.Invoke();
    public void ViewModel(T item) => ViewPopupRequested?.Invoke(item);
    public void EditModel(T item) => EditPopupRequested?.Invoke(item);
    public void ClosePopup() => PopupCloseRequested?.Invoke();
}