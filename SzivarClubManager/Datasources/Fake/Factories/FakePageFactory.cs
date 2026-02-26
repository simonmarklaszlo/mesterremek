using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SzivarClubManager.Datasources.Fake.Factories;

public abstract class FakePageFactory<T> : IPageFactory<T>
{
    private readonly List<T> _data;

    protected FakePageFactory(List<T> data)
    {
        _data = data;
    }

    public Task<bool> PageExists(int page, int pageSize) => Task.FromResult(_data.Count > (page - 1) * pageSize);
    public Task<int> GetLastPage(int pageSize) => _data.Count < pageSize ? Task.FromResult(1) : Task.FromResult((int)Math.Ceiling((float)_data.Count / pageSize));
    public Task<T[]> GetPage(int page, int pageSize) => Task.FromResult(_data.Skip((page - 1) * pageSize).Take(pageSize).ToArray());

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
}