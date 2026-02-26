using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SzivarClubManager.Configs;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Helper;
using SzivarClubManager.Models;
using SzivarClubManager.ViewModels.Activities.Page.Navigation;

namespace SzivarClubManager.ViewModels.Activities.Page.Data;

public abstract partial class PageDataViewModel<T> : ViewModelBase where T : class, IModel
{
    public abstract T[] CurrentPageData { get; protected set; }
    [ObservableProperty] private int _currentPage = 1;
    public abstract T[] SelectedItems { get; set; }

    [ObservableProperty] private bool _isFirstPageButtonEnabled;
    [ObservableProperty] private bool _isPreviousPageButtonEnabled;
    [ObservableProperty] private bool _isNextPageButtonEnabled;
    [ObservableProperty] private bool _isLastPageButtonEnabled;


    private const string DeleteTextSingle = "Delete";
    private const string DeleteTextMultiple = "Delete selected";
    private const string UndeleteTextSingle = "Undelete";
    private const string UndeleteTextMultiple = "Undelete selected";
    [ObservableProperty] private string _menuTextSingleDelete = DeleteTextSingle;
    [ObservableProperty] private string _menuTextMultipleDelete = DeleteTextMultiple;

    private readonly IPageFactory<T> _factory;
    private readonly PageController<T> _controller;


    protected PageDataViewModel(IPageFactory<T> factory, PageController<T> controller)
    {
        _factory = factory;
        _controller = controller;
    }

    public Task InitializeAsync() => LoadCurrentPage();

    private async Task LoadCurrentPage()
    {
        CurrentPageData = await _factory.GetPage(CurrentPage, GlobalConfig.Instance.UserPreferences.PageSize);

        IsFirstPageButtonEnabled = CurrentPage > 1;
        IsPreviousPageButtonEnabled = CurrentPage > 1;
        int lastPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
        IsNextPageButtonEnabled = lastPage > CurrentPage;
        IsLastPageButtonEnabled = lastPage > CurrentPage;
    }

    [RelayCommand]
    private async Task LoadPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadNextPage()
    {
        if (await _factory.PageExists(CurrentPage + 1, GlobalConfig.Instance.UserPreferences.PageSize))
        {
            CurrentPage++;
            await LoadCurrentPage();
        }
    }

    [RelayCommand]
    private async Task LoadFirstPage()
    {
        CurrentPage = 1;
        await LoadCurrentPage();
    }

    [RelayCommand]
    private async Task LoadLastPage()
    {
        CurrentPage = await _factory.GetLastPage(GlobalConfig.Instance.UserPreferences.PageSize);
        await LoadCurrentPage();
    }

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private async Task CopyCell() => await Clipboard.CopyText(SelectedItems[0].ToString());

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private async Task CopyRecord() => await Clipboard.CopyText(SelectedItems[0].ToCopiableString());

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private void ViewRecord() => _controller.SelectItem(SelectedItems[0], false);

    [RelayCommand(CanExecute = nameof(SingleCommandCanExecute))]
    private void EditRecord() => _controller.SelectItem(SelectedItems[0], true);

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

        CurrentPageData = CurrentPageData.ToArray();
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

        CurrentPageData = CurrentPageData.ToArray();
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
            MenuTextSingleDelete = UndeleteTextSingle;
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

            MenuTextMultipleDelete = UndeleteTextMultiple;
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
}