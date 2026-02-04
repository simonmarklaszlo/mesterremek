using System;
using System.Collections.Generic;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Datasources.Pagination;

namespace SzivarClubManager.Datasources;

public partial class FactoryProvider
{
    private readonly Dictionary<Type, IFactory> _factories;

    private FactoryProvider(Dictionary<Type, IFactory> factories)
    {
        _factories = factories;
    }

    public IPageFactory<T> GetPageFactory<T>()
    {
        Type s = typeof(T);
        IFactory? factory = _factories.GetValueOrDefault(s);
        if (factory is IPageFactory<T> pageFactory) return pageFactory;

        throw new InvalidOperationException($"No factory for type {s.FullName}");
    }


    public static FactoryProvider Create(DatabaseConnection connection) => new(GetGeneratedFactoriesMap(connection));
    public static FactoryProvider Fake() => new(GetGeneratedFakeFactoriesMap());


    private static partial Dictionary<Type, IFactory> GetGeneratedFactoriesMap(DatabaseConnection connection);
    private static partial Dictionary<Type, IFactory> GetGeneratedFakeFactoriesMap();
}