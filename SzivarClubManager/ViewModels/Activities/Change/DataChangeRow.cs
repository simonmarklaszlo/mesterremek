using CommunityToolkit.Mvvm.ComponentModel;

namespace SzivarClubManager.ViewModels.Activities.Change;

public abstract class DataChangeRow : ObservableObject
{
    public int AddCount
    {
        get;
        protected set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int EditCount
    {
        get;
        protected set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int DeleteCount
    {
        get;
        protected set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public abstract string TypeName { get; }

    public abstract void Refresh();
}