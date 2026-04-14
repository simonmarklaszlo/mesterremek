using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.ViewModels.Activities.Page.Filter;
using SzivarClubManager.ViewModels.Activities.Page.MinimalFilter;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public abstract partial class PageDataViewModel<TModel, TFilter> : ViewModelBase
    where TModel : class, IModel
    where TFilter : IFilter<TModel>
{
    public abstract TModel[] CurrentPageData { get; protected set; }
    [ObservableProperty] private int _currentPage = 1;

    public DateTime LastGlobalChange => Changes.LastChange;

    public IList? SelectedItemsRaw
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) SelectedItems = value?.Cast<TModel>().ToArray() ?? [];
        }
    }

    public abstract TModel[] SelectedItems { get; set; }
    public object? SelectedItem { get; set; }
    public DataGridColumn? CurrentColumn { get; set; }

    private bool CanGoToFirstPage { get; set; }
    private bool CanGoToPreviousPage { get; set; }
    private bool CanGoToNextPage { get; set; }
    private bool CanGoToLastPage { get; set; }


    private const string DeleteTextSingle = "Törlés";
    private const string DeleteTextMultiple = "Kijelöltek törlése";
    private const string UndoDeleteTextSingle = "Törlés visszavonása";
    private const string UndoDeleteTextMultiple = "Kijelöltek törlésének visszavonása";
    [ObservableProperty] private string _menuTextSingleDelete = DeleteTextSingle;
    [ObservableProperty] private string _menuTextMultipleDelete = DeleteTextMultiple;

    private readonly IPageFactory<TModel> _factory;
    private readonly PopupService _popupService;
    private readonly PageController<TModel> _controller;
    private Task? _loadCurrentPageTask;

    protected abstract TFilter Filter { get; }
    public abstract MinimalFilterViewModel<TFilter, TModel> MinimalFilterViewModel { get; }
    public abstract FilterViewModel<TFilter, TModel> FilterViewModel { get; }

    protected PageDataViewModel(PopupService popupService, PageController<TModel> controller, IPageFactory<TModel> factory)
    {
        _popupService = popupService;
        _controller = controller;
        _factory = factory;

        _controller.PopupCloseRequested += RefreshChangedData;
    }

    public Task InitializeAsync() => LoadCurrentPage();

    public override void OnOpening() => _ = LoadCurrentPage();

    private Task LoadCurrentPage()
    {
        if (_loadCurrentPageTask is { IsCompleted: false }) return _loadCurrentPageTask;

        _loadCurrentPageTask = LoadCurrentPageCore();
        return _loadCurrentPageTask;
    }

    private async Task LoadCurrentPageCore()
    {
        CurrentPageData = await _factory.GetPage(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize, Filter);

        await SetButtonStates();
    }

    private async Task SetButtonStates()
    {
        CanGoToFirstPage = CurrentPage > 1;
        CanGoToPreviousPage = CurrentPage > 1;
        int lastPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize, Filter);
        CanGoToNextPage = lastPage > CurrentPage;
        CanGoToLastPage = lastPage > CurrentPage;

        LoadFirstPageCommand.NotifyCanExecuteChanged();
        LoadPreviousPageCommand.NotifyCanExecuteChanged();
        LoadNextPageCommand.NotifyCanExecuteChanged();
        LoadLastPageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task LoadPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCurrentPage();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task LoadNextPage()
    {
        if (await _factory.PageExists(CurrentPage + 1, GlobalConfig.Instance.UserPreferences.PageSize, Filter))
        {
            CurrentPage++;
            await LoadCurrentPage();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoToFirstPage))]
    private async Task LoadFirstPage()
    {
        CurrentPage = 1;
        await LoadCurrentPage();
    }

    [RelayCommand(CanExecute = nameof(CanGoToLastPage))]
    private async Task LoadLastPage()
    {
        CurrentPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize, Filter);
        await LoadCurrentPage();
    }

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private async Task CopyCell()
    {
        if (SelectedItem == null || CurrentColumn == null) return;

        if (CurrentColumn is DataGridTextColumn { Binding: CompiledBindingExtension binding })
        {
            var s = binding.Path.ToString();
            var prop = SelectedItem.GetType().GetProperty(s);
            var value = prop?.GetValue(SelectedItem);

            if (value is not null)
            {
                await Clipboard.CopyText(value.ToString()!);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private async Task CopyRecord() => await Clipboard.CopyText(SelectedItems[0].ToCopiableString());

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private void ViewRecord() => _controller.ViewModel(SelectedItems[0]);

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private void EditRecord() => _controller.EditModel(SelectedItems[0]);

    [RelayCommand]
    private void AddRecord() => _controller.AddModel();

    [RelayCommand(CanExecute = nameof(SingleDeleteCanExecute))]
    private void DeleteRecord()
    {
        if (Changes.IsDeleted(SelectedItems[0]))
        {
            Changes.RemoveDeleted(SelectedItems[0]);
        }
        else
        {
            Changes.Delete(SelectedItems[0]);
        }

        RefreshChangedData();
    }

    [RelayCommand(CanExecute = nameof(MultipleDeleteCanExecute))]
    private void DeleteSelected()
    {
        if (Changes.IsDeleted(SelectedItems[0]))
        {
            Changes.RemoveDeleted(SelectedItems);
        }
        else
        {
            Changes.Delete(SelectedItems);
        }

        RefreshChangedData();
    }

    private bool SingleCommandCanExecute() => SelectedItems.Length == 1;

    private bool SingleDeleteCanExecute()
    {
        if (SelectedItems.Length != 1)
        {
            MenuTextSingleDelete = DeleteTextSingle;
            return false;
        }

        var item = SelectedItems[0];
        if (!Changes.IsDeleted(item))
        {
            MenuTextSingleDelete = DeleteTextSingle;
            return true;
        }

        if (Changes.IsDeleted(item))
        {
            MenuTextSingleDelete = UndoDeleteTextSingle;
            return true;
        }

        MenuTextSingleDelete = DeleteTextSingle;
        return false;
    }

    private bool MultipleDeleteCanExecute()
    {
        if (SelectedItems.Length <= 1)
        {
            MenuTextMultipleDelete = DeleteTextMultiple;
            return false;
        }

        if (SelectedItems.Any(x => Changes.IsDeleted(x)))
        {
            if (SelectedItems.Any(x => !Changes.IsDeleted(x)))
            {
                MenuTextMultipleDelete = DeleteTextMultiple;
                return false;
            }

            MenuTextMultipleDelete = UndoDeleteTextMultiple;
            return true;
        }

        MenuTextMultipleDelete = DeleteTextMultiple;
        return true;
    }

    protected void OnSelectedItemsChanged()
    {
        CopyCellCommand.NotifyCanExecuteChanged();
        CopyRecordCommand.NotifyCanExecuteChanged();
        ViewRecordCommand.NotifyCanExecuteChanged();
        EditRecordCommand.NotifyCanExecuteChanged();
        DeleteRecordCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void ShowFilterPopup()
    {
        FilterViewModel.RefreshFilterFields();
        _popupService.ShowPopup(FilterViewModel);
    }

    [RelayCommand]
    private async Task TriggerSearch()
    {
        MinimalFilterViewModel.SearchText = Filter.ToString();
        CurrentPage = 1;
        await LoadCurrentPage();
    }

    private void RefreshChangedData()
    {
        CurrentPageData = [.. CurrentPageData];
    }
}