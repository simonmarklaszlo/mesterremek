using Avalonia.Controls;

namespace SzivarClubManager.Views.Activities.Page.Data;

public partial class CigarPageDataView : UserControl
{
    public CigarPageDataView()
    {
        InitializeComponent();

        DataGrid.AutoGeneratingColumn += (_, e) =>
        {
            e.Column.Header = e.PropertyName;
            e.Column.Tag = e.PropertyName;
        };
    }


}