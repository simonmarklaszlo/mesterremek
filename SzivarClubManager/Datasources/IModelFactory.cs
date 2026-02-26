using System.Collections.Generic;
using System.Threading.Tasks;

namespace SzivarClubManager.Datasources;

public interface IModelFactory<in T> : IFactory
{
    public Task<int> AddRange(IEnumerable<T> items);
    public Task<int> EditRange(IEnumerable<T> items);
    public Task<int> DeleteRange(IEnumerable<T> items);
}