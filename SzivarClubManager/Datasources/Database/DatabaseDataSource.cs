using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Database;

public class DatabaseDataSource : IDataSource
{
    private readonly DatabaseConnection _connection;

    public DatabaseDataSource(DatabaseConnection connection)
    {
        _connection = connection;
    }

    private async Task<bool> PageExitst(string table, int page, int pageSize)
    {
        const string query = "SELECT EXISTS ( SELECT 1 FROM @table LIMIT @limit OFFSET @offset )";

        await using var command = new NpgsqlCommand(query, _connection.Connection);
        command.Parameters.AddWithValue("table", table);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        return (bool)(await command.ExecuteScalarAsync() ?? Task.FromResult(false));
    }

    private async Task<int> GetLastPage(string table, int pageSize)
    {
        const string query = "SELECT COUNT(*) FROM @table";

        await using var command = new NpgsqlCommand(query, _connection.Connection);
        command.Parameters.AddWithValue("table", table);

        int count = Convert.ToInt32(await command.ExecuteScalarAsync());

        return (int)Math.Ceiling((float)count / pageSize);
    }


    public Task<bool> UserPageExists(int page, int pageSize) => PageExitst("users", page, pageSize);
    public Task<int> GetLastUserPage(int pageSize) => GetLastPage("users", pageSize);

    public async Task<User[]> GetUserPage(int page, int pageSize)
    {
        const string query = """
                             SELECT u.id, u.name, u.email, u.created_at, r.id, r.name
                             FROM users u 
                             JOIN roles r ON u.role_id = r.id 
                             LIMIT @limit OFFSET @offset
                             """;


        await using var command = new NpgsqlCommand(query, _connection.Connection);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<int, Role> roles = [];
        List<User> users = [];

        while (await reader.ReadAsync())
        {
            int userId = reader.GetInt32(0);
            string userName = reader.GetString(1);
            string userEmail = reader.GetString(2);
            DateTime userCreatedAt = reader.GetDateTime(3);
            int roleId = reader.GetInt32(4);
            string roleName = reader.GetString(5);

            if (!roles.TryGetValue(roleId, out Role? value))
            {
                value = new Role(roleId, roleName);
                roles.Add(roleId, value);
            }

            users.Add(new User(userId, userName, userEmail, userCreatedAt, value));
        }

        return users.ToArray();
    }


    public Task<bool> CigarPageExists(int page, int pageSize) => PageExitst("cigars", page, pageSize);
    public Task<int> GetLastCigarPage(int pageSize) => GetLastPage("cigars", pageSize);

    public async Task<Cigar[]> GetCigarPage(int page, int pageSize)
    {
        const string query = """
                             SELECT c.id, c.name, b.id, b.name
                             FROM cigars c 
                             JOIN roles b ON c.brand_id = b.id 
                             LIMIT @limit OFFSET @offset
                             """;

        await using var command = new NpgsqlCommand(query, _connection.Connection);
        command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("limit", pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<int, Brand> brands = [];
        List<Cigar> cigars = [];

        while (await reader.ReadAsync())
        {
            int cigarId = reader.GetInt32(0);
            string cigarName = reader.GetString(1);
            int brandId = reader.GetInt32(2);
            string brandName = reader.GetString(3);

            if (!brands.TryGetValue(brandId, out Brand? value))
            {
                value = new Brand(brandId, brandName);
                brands.Add(brandId, value);
            }

            cigars.Add(new Cigar(cigarId, cigarName, value));
        }

        return cigars.ToArray();
    }
}