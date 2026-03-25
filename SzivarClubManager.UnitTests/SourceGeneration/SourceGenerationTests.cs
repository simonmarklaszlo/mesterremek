using System.Collections.ObjectModel;
using SzivarClubManager.Datasources.Change;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Services;
using SzivarClubManager.SourceGeneration.Generated;
using SzivarClubManager.ViewModels.Activities.Change;

namespace SzivarClubManager.UnitTests.SourceGeneration;

public class SourceGenerationTests
{
    [Fact]
    public void ActivityCollection_ShouldNotBeEmpty()
    {
        FactoryProvider factoryProvider = MockFactoryProvider();
        PopupService popupService = new PopupService(null!);


        var activities = ActivityCollection.GetActivities(factoryProvider, popupService);

        Assert.NotEmpty(activities);
    }

    [Fact]
    public void Changes_Initialize_ShouldPopulateDataChanges()
    {
        var provider = MockFactoryProvider();

        Changes.Initialize(provider);

        var cigarChanges = InvokeGetDataChange<Cigar>();
        Assert.NotNull(cigarChanges);
    }

    [Fact]
    public void ChangesActivityViewModel_GetDataChangeRows_ShouldNotBeEmpty()
    {
        var obj = typeof(ChangesActivityViewModel)
            .GetMethod("GetDataChangeRows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);

        var rows = obj as ObservableCollection<DataChangeRow>;

        Assert.NotNull(rows);
        Assert.NotEmpty(rows);
    }

    [Fact]
    public void FactoryProvider_GetGeneratedFactoriesMap_ShouldNotBeEmpty()
    {
        var obj = typeof(FactoryProvider)
            .GetMethod("GetGeneratedFactoriesMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [null]);

        var dict = obj as Dictionary<Type, IFactory>;

        Assert.NotNull(dict);
        Assert.NotEmpty(dict);
    }

    private static object InvokeGetDataChange<T>() where T : class, IModel
    {
        var method = typeof(Changes)
            .GetMethod("GetDataChange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(typeof(T));

        return method.Invoke(null, null)!;
    }

    private static FactoryProvider MockFactoryProvider()
    {
        FactoryProvider.CreateDatabase(null!);
        return FactoryProvider.Instance;
    }
}