using System;
using System.Linq;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Mock.Factories;

[MockFactoryOf(typeof(User))]
public class MockUserFactory : MockFactory<User>
{
    public MockUserFactory() : base(GetData()) { }

    private static User[] GetData()
    {
        Random random = new(0);
        Role[] roles = [new(1, "Admin"), new(2, "User")];

        return Enumerable.Range(0, 125)
            .Select(x => new User(x, Guid.NewGuid().ToString(), "email", DateTime.Now.AddDays(x), roles[random.Next(0, roles.Length)]))
            .ToArray();
    }
}