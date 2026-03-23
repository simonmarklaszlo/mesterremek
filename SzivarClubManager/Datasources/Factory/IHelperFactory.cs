using System;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

/// <summary>
/// Defines helper operations for retrieving and caching model collections.
/// </summary>
/// <typeparam name="T">The model type handled by the factory.</typeparam>
public interface IHelperFactory<T> : IModelFactory<T> where T : IModel
{
    /// <summary>
    /// Gets the last cache update time.
    /// </summary>
    DateTime CacheUpdated { get; }

    /// <summary>
    /// Gets all models from the underlying data source.
    /// </summary>
    /// <returns>An array containing all available models.</returns>
    Task<T[]> GetAll();

    /// <summary>
    /// Tries to get all models from cache.
    /// If the cache is empty, the data source is queried and the result is cached.
    /// </summary>
    /// <returns>An array of cached models.</returns>
    Task<T[]> TryGetAllFromCache();

    /// <summary>
    /// Tries to get all models from cache, ensuring the result contains the model with specified id.
    /// If the cache is empty or the model is missing, the data source is queried and the result is cached.
    /// </summary>
    /// <param name="mustContainId">The identifier that must be present in the cached result.</param>
    /// <returns>An array of cached models that satisfies the identifier requirement.</returns>
    Task<T[]> TryGetAllFromCache(int mustContainId);

    /// <summary>
    /// Invalidates the current cache.
    /// </summary>
    void InvalidateCache();
}