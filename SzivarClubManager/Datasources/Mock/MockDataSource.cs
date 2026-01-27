using System;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Mock;

public class MockDataSource : IDataSource
{
    private readonly User[] _users;
    private readonly Cigar[] _cigars;

    public MockDataSource()
    {
        Role[] roles = [new(1, "Admin"), new(2, "User")];
        Brand[] brands = [new(1, "valami")];

        _users = Enumerable.Range(0, 125)
            .Select(x => new User(x, Guid.NewGuid().ToString(), "email", DateTime.Now.AddDays(x), roles[1]))
            .ToArray();

        _cigars = Enumerable.Range(0, 125)
            .Select(x => new Cigar(x, Guid.NewGuid().ToString(), brands[0]))
            .ToArray();
    }

    public Task<bool> UserPageExists(int page, int pageSize) => Task.FromResult(_users.Length > (page - 1) * pageSize);
    public Task<int> GetLastUserPage(int pageSize) => _users.Length < pageSize ? Task.FromResult(1) : Task.FromResult((int)Math.Ceiling((float)_users.Length / pageSize));
    public Task<User[]> GetUserPage(int page, int pageSize) => Task.FromResult(_users.Skip((page - 1) * pageSize).Take(pageSize).ToArray());


    public Task<bool> CigarPageExists(int page, int pageSize) => Task.FromResult(_cigars.Length > (page - 1) * pageSize);
    public Task<int> GetLastCigarPage(int pageSize) => _cigars.Length < pageSize ? Task.FromResult(1) : Task.FromResult((int)Math.Ceiling((float)_cigars.Length / pageSize));
    public Task<Cigar[]> GetCigarPage(int page, int pageSize) => Task.FromResult(_cigars.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
}