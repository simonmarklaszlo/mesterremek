using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SzivarClubManager.ViewModels.Activities;

public abstract partial class PageActivityBaseViewModel : ActivityViewModel
{
    [ObservableProperty] private IEnumerable _data = Array.Empty<object>();

    [ObservableProperty] private int _currentPage = 1;

    [ObservableProperty] private bool _isFirstPageButtonEnabled;
    [ObservableProperty] private bool _isPreviousPageButtonEnabled;
    [ObservableProperty] private bool _isNextPageButtonEnabled;
    [ObservableProperty] private bool _isLastPageButtonEnabled;

    public abstract IRelayCommand LoadFirstPageCmd { get; }
    public abstract IRelayCommand LoadPreviousPageCmd { get; }
    public abstract IRelayCommand LoadNextPageCmd { get; }
    public abstract IRelayCommand LoadLastPageCmd { get; }
}