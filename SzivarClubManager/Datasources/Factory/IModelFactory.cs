using System.Collections.Generic;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

/// <summary>
/// Defines batch operations for model instances.
/// </summary>
/// <typeparam name="T">The model type handled by the factory.</typeparam>
public interface IModelFactory<in T> : IFactory where T : IModel
{
    /// <summary>
    /// Adds a collection of models.
    /// </summary>
    /// <param name="items">The models to add.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> AddRange(IEnumerable<T> items);

    /// <summary>
    /// Updates a collection of models.
    /// </summary>
    /// <param name="items">The models to update.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> EditRange(IEnumerable<T> items);

    /// <summary>
    /// Deletes a collection of models.
    /// </summary>
    /// <param name="items">The models to delete.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> DeleteRange(IEnumerable<T> items);
}