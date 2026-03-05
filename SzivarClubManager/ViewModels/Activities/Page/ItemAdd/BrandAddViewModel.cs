using CommunityToolkit.Mvvm.ComponentModel;
using SzivarClubManager.Datasources;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Models;

namespace SzivarClubManager.ViewModels.Activities.Page.ItemAdd;

public sealed partial class BrandAddViewModel : ItemAddViewModel<Brand>
{
    [ObservableProperty] private string _name = string.Empty;
    protected override bool CanAdd => !string.IsNullOrWhiteSpace(Name);


    partial void OnNameChanged(string value) => ReCheckCommandCanExecute();

    protected override void ResetFields()
    {
        Name = string.Empty;
    }

    protected override void AddNewItemToChanges()
    {
        Brand newItem = Brand.CreateNew(Name);
        Changes.AddNew(newItem);
    }
}