using System;
using System.Threading.Tasks;

namespace SzivarClubManager.Datasources;

public interface IHelperFactory<T> : IModelFactory<T>
{
    public DateTime CacheUpdated { get; }
    Task<T[]> GetAll();
    Task<T[]> TryGetAllFromCache();
    Task<T[]> TryGetAllFromCache(int mustContainId);
    void InvalidateCache();
}