using System.Threading.Tasks;

namespace SzivarClubManager.Datasources;

public interface IHelperFactory<T> : IModelFactory<T>
{
    Task<T[]> GetAll();
    Task<T[]> TryGetAllFromCache();
    Task<T[]> TryGetAllFromCache(int mustContainId);
}