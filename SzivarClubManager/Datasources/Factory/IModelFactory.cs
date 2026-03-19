using System.Collections.Generic;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Factory;

public interface IModelFactory<in T> : IFactory where T : IModel
{
    public Task<int> AddRange(IEnumerable<T> items);
    public Task<int> EditRange(IEnumerable<T> items);
    public Task<int> DeleteRange(IEnumerable<T> items);
}