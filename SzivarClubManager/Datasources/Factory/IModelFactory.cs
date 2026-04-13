using System.Collections.Generic;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

/// <summary>
/// Defines batch operations for model instances.
/// </summary>
/// <typeparam name="T">The model type handled by the factory.</typeparam>
public interface IModelFactory<T> : IFactory where T : IModel
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

    /// <summary>
    /// Gets a model by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the model to retrieve.</param>
    /// <returns>
    /// A model instance when found; otherwise <see langword="null"/>.
    /// </returns>
    Task<T?> GetModel(int id);

    /// <summary>
    /// Gets models for the provided identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the models to retrieve.</param>
    /// <returns>An array containing the matching models.</returns>
    Task<T[]> GetModel(IEnumerable<int> ids);
}