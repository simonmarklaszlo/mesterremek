using SzivarClubManager.Datasources;

namespace SzivarClubManager.ViewModels.Activities;

public abstract class ActivityViewModel : ViewModelBase
{
    protected readonly IDataSource DataSource;

    protected ActivityViewModel(IDataSource dataSource)
    {
        DataSource = dataSource;
    }
}