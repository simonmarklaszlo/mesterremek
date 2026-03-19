using System;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

public interface IHelperFactory<T> : IModelFactory<T> where T : IModel
{
    public DateTime CacheUpdated { get; }
    Task<T[]> GetAll();
    Task<T[]> TryGetAllFromCache();
    Task<T[]> TryGetAllFromCache(int mustContainId);
    void InvalidateCache();
}