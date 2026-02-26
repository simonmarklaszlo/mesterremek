using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SzivarClubManager.Datasources.Fake.Factories;

public abstract class FakeHelperFactory<T> : IHelperFactory<T>
{
    private readonly List<T> _data;

    protected FakeHelperFactory(List<T> data)
    {
        _data = data;
    }

    public Task<int> AddRange(IEnumerable<T> items)
    {
        var arr = items as T[] ?? items.ToArray();
        _data.AddRange(arr);
        return Task.FromResult(arr.Length);
    }

    public Task<int> EditRange(IEnumerable<T> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            int index = _data.IndexOf(item);
            _data[index] = item;
            count++;
        }

        return Task.FromResult(count);
    }

    public Task<int> DeleteRange(IEnumerable<T> items) => Task.FromResult(_data.RemoveAll(items.Contains));

    public Task<T[]> GetAll() => Task.FromResult(_data.ToArray());

    public Task<T[]> TryGetAllFromCache() => GetAll();
    public Task<T[]> TryGetAllFromCache(int mustContainId) => GetAll();
}