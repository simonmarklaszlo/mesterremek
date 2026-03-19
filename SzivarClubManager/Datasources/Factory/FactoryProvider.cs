using System;
using System.Collections.Generic;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

public sealed partial class FactoryProvider
{
    private readonly Dictionary<Type, IFactory> _factories;

    private FactoryProvider(Dictionary<Type, IFactory> factories)
    {
        _factories = factories;
    }

    public IFactory GetFactory<T>() where T : IModel => _factories[typeof(T)];

    public IPageFactory<T> GetPageFactory<T>() where T : class, IModel
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IPageFactory<T> pageFactory) return pageFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    public IHelperFactory<T> GetHelperFactory<T>() where T : IModel
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IHelperFactory<T> helperFactory) return helperFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    public static FactoryProvider Instance { get; private set; } = null!;
    public static void CreateDatabase(DatabaseConnection connection) => Instance = new FactoryProvider(GetGeneratedFactoriesMap(connection));
    private static partial Dictionary<Type, IFactory> GetGeneratedFactoriesMap(DatabaseConnection connection);
}