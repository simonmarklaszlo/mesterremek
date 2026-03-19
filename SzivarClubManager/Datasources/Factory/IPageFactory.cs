using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

public interface IPageFactory<T> : IModelFactory<T> where T : class, IModel
{
    Task<bool> PageExists(int page, int pageSize, IFilter<T> filter);
    Task<int> GetLastPage(int pageSize, IFilter<T> filter);
    Task<T[]> GetPage(int page, int pageSize, IFilter<T> filter);
}