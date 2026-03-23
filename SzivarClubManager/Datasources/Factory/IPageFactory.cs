using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

/// <summary>
/// Defines paged query operations for model instances.
/// </summary>
/// <typeparam name="T">The model type handled by the factory.</typeparam>
public interface IPageFactory<T> : IModelFactory<T> where T : class, IModel
{
    /// <summary>
    /// Determines whether the specified page exists for the given filter.
    /// </summary>
    /// <param name="page">The 1-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="filter">The filter applied to the query.</param>
    /// <returns><c>true</c> if the page contains at least one item; otherwise, <c>false</c>.</returns>
    Task<bool> PageExists(int page, int pageSize, IFilter<T> filter);

    /// <summary>
    /// Gets the last available page index for the given filter.
    /// </summary>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="filter">The filter applied to the query.</param>
    /// <returns>The last available 1-based page index.</returns>
    Task<int> GetLastPage(int pageSize, IFilter<T> filter);

    /// <summary>
    /// Gets the items for the specified page.
    /// </summary>
    /// <param name="page">The 1-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="filter">The filter applied to the query.</param>
    /// <returns>An array containing the models on the requested page.</returns>
    Task<T[]> GetPage(int page, int pageSize, IFilter<T> filter);
}