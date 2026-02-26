using System;
using System.Collections.Generic;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Datasources.Fake.Factories;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources;

public sealed partial class FactoryProvider
{
    private readonly Dictionary<Type, IFactory> _factories;

    private FactoryProvider(Dictionary<Type, IFactory> factories)
    {
        _factories = factories;
    }

    public IPageFactory<T> GetPageFactory<T>()
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IPageFactory<T> pageFactory) return pageFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    public IHelperFactory<T> GetHelperFactory<T>()
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IHelperFactory<T> helperFactory) return helperFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    public static FactoryProvider Instance { get; private set; } = null!;
    public static void CreateDatabase(DatabaseConnection connection) => Instance = new FactoryProvider(GetGeneratedFactoriesMap(connection));
    public static void CreateFake() => Instance = new FactoryProvider(GetGeneratedFakeFactoriesMap());


    private static partial Dictionary<Type, IFactory> GetGeneratedFactoriesMap(DatabaseConnection connection);
    private static partial Dictionary<Type, IFactory> GetGeneratedFakeFactoriesMap();
}