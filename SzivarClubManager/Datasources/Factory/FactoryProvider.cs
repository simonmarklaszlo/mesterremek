using System;
using System.Collections.Generic;
using SzivarClubManager.Datasources.Database;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

/// <summary>
/// Provides access to registered model factories and initializes the factory registry.
/// </summary>
public sealed partial class FactoryProvider
{
    /// <summary>
    /// Stores factories keyed by the model type they create.
    /// </summary>
    private readonly Dictionary<Type, IFactory> _factories;

    private FactoryProvider(Dictionary<Type, IFactory> factories)
    {
        _factories = factories;
    }

    /// <summary>
    /// Gets the factory registered for model type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Model type associated with a factory.</typeparam>
    /// <returns>The registered factory instance.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no factory is registered for <typeparamref name="T"/>.</exception>
    public IFactory GetFactory<T>() where T : IModel => _factories[typeof(T)];

    /// <summary>
    /// Gets a page factory for model type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Model type associated with a page factory.</typeparam>
    /// <returns>The registered page factory instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no page factory is registered for <typeparamref name="T"/>.</exception>
    public IPageFactory<T> GetPageFactory<T>() where T : class, IModel
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IPageFactory<T> pageFactory) return pageFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    /// <summary>
    /// Gets a helper factory for model type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Model type associated with a helper factory.</typeparam>
    /// <returns>The registered helper factory instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no helper factory is registered for <typeparamref name="T"/>.</exception>
    public IHelperFactory<T> GetHelperFactory<T>() where T : IModel
    {
        IFactory? factory = _factories.GetValueOrDefault(typeof(T));
        if (factory is IHelperFactory<T> helperFactory) return helperFactory;

        throw new InvalidOperationException($"No factory for type {typeof(T).FullName}");
    }

    /// <summary>
    /// Gets the singleton factory provider instance after initialization.
    /// </summary>
    public static FactoryProvider Instance { get; private set; } = null!;

    /// <summary>
    /// Initializes the <see cref="Instance"/> using the provided database connection.
    /// </summary>
    /// <param name="connection">Active database connection used to construct factories.</param>
    public static void CreateDatabase(DatabaseConnection connection) => Instance = new FactoryProvider(GetGeneratedFactoriesMap(connection));

    /// <summary>
    /// Gets the generated map of model types and their factories.
    /// </summary>
    /// <param name="connection">Active database connection used to construct factories.</param>
    /// <returns>A dictionary containing all generated factories.</returns>
    private static partial Dictionary<Type, IFactory> GetGeneratedFactoriesMap(DatabaseConnection connection);
}