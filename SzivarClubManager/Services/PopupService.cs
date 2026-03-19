using SzivarClubManager.ViewModels;

namespace SzivarClubManager.Services;

public class PopupService
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public PopupService(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void ShowPopup(ViewModelBase viewModel)
    {
        _mainWindowViewModel.PopupViewModel = viewModel;
    }

    public void ClosePopup()
    {
        _mainWindowViewModel.PopupViewModel = null;
    }
}