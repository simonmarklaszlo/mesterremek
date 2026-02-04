using System;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Pagination;

namespace SzivarClubManager.Datasources.Mock.Factories;

public abstract class MockFactory<T> : IPageFactory<T>, IMockObject
{
    private readonly T[] _data;

    protected MockFactory(T[] data)
    {
        _data = data;
    }

    public Task<bool> PageExists(int page, int pageSize) => Task.FromResult(_data.Length > (page - 1) * pageSize);
    public Task<int> GetLastPage(int pageSize) => _data.Length < pageSize ? Task.FromResult(1) : Task.FromResult((int)Math.Ceiling((float)_data.Length / pageSize));
    public Task<T[]> GetPage(int page, int pageSize) => Task.FromResult(_data.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
}