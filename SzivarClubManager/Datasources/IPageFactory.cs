using System.Threading.Tasks;

namespace SzivarClubManager.Datasources;

public interface IPageFactory<T> : IFactory
{
    Task<bool> PageExists(int page, int pageSize);
    Task<int> GetLastPage(int pageSize);
    Task<T[]> GetPage(int page, int pageSize);
}