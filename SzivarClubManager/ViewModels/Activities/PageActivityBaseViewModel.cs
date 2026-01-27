using System.Collections;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Datasources;

namespace SzivarClubManager.ViewModels.Activities;

public abstract class PageActivityBaseViewModel(IDataSource dataSource) : ActivityViewModel(dataSource)
{
    // ===== Bound properties =====
    public abstract IEnumerable Data { get; }

    public abstract int CurrentPage { get; }

    public abstract bool IsFirstPageButtonEnabled { get; }
    public abstract bool IsPreviousPageButtonEnabled { get; }
    public abstract bool IsNextPageButtonEnabled { get; }
    public abstract bool IsLastPageButtonEnabled { get; }

    // ===== Bound commands =====
    public abstract IRelayCommand LoadFirstPageCmd { get; }
    public abstract IRelayCommand LoadPreviousPageCmd { get; }
    public abstract IRelayCommand LoadNextPageCmd { get; }
    public abstract IRelayCommand LoadLastPageCmd { get; }
}