namespace SzivarClubManager.ViewModels.AppState;

public class ErrorViewModel : ViewModelBase
{
    public string ErrorText { get; }

    public ErrorViewModel(string errorText)
    {
        ErrorText = errorText;
    }
}