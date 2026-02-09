using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(User), true)]
public class UserFactory : IPageFactory<User>
{
    private const string TableName = "users";
    private readonly DatabaseConnection _connection;

    public UserFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public Task<bool> PageExists(int page, int pageSize) => _connection.PageExitstOnTable(TableName, page, pageSize);

    public Task<int> GetLastPage(int pageSize) => _connection.GetLastPageOnTable(TableName, pageSize);

    public async Task<User[]> GetPage(int page, int pageSize)
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
}