namespace SzivarClubManager.SourceGenerator;

public static class StringReferences
{
    // Factory
    public const string FactoryAttribute = "SzivarClubManager.SourceGeneration.Factory.FactoryAttribute";
    public const string FactoryInterface = "SzivarClubManager.Datasources.Factory.IFactory";
    public const string ModelFactoryInterface = "SzivarClubManager.Datasources.Factory.IModelFactory`1";
    public const string PageFactoryInterface = "SzivarClubManager.Datasources.Factory.IPageFactory`1";
    public const string HelperFactoryInterface = "SzivarClubManager.Datasources.Factory.IHelperFactory`1";
    public const string FactoryProvider = "SzivarClubManager.Datasources.Factory.FactoryProvider";


    // Model
    public const string ModelInterface = "SzivarClubManager.Models.IModel";
    public const string ModelAttribute = "SzivarClubManager.SourceGeneration.Model.ModelAttribute";


    // Database
    public const string DatabaseConnection = "SzivarClubManager.Datasources.Database.DatabaseConnection";


    // Changes
    public const string Changes = "global::SzivarClubManager.Datasources.Change.Changes";
    public const string DataChanges = "SzivarClubManager.Datasources.Change.DataChanges";
    public const string DataChangesInterface = "SzivarClubManager.Datasources.Change.IDataChanges";
    public const string DataChangeRow = "SzivarClubManager.ViewModels.Activities.Change.DataChangeRow";


    // Activity
    public const string ActivityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.Activity.ActivityCollectionItemAttribute";
    public const string PageActivityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.Activity.PageActivityCollectionItemAttribute";
    public const string ActivityViewModel = "SzivarClubManager.ViewModels.Activities.ActivityViewModel";


    // Services
    public const string PopupService = "SzivarClubManager.Services.PopupService";


    // Filter
    public const string FilterValueHandler = "SzivarClubManager.Datasources.Database.Filters.FilterValueHandler";
    public const string FilterAttribute = "SzivarClubManager.SourceGeneration.Filter.ModelFilterAttribute";
    public const string FilterPropertyAttribute = "SzivarClubManager.SourceGeneration.Filter.ModelFilterPropertyAttribute";
    public const string FilterViewModelAttribute = "SzivarClubManager.SourceGeneration.Filter.ModelFilterViewModelAttribute";
    public const string FilterPredicateOfProperty = "SzivarClubManager.SourceGeneration.Filter.PredicateOfPropertyAttribute";
}