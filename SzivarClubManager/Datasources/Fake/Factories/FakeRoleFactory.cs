using System.Collections.Generic;
using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(Role))]
public sealed class FakeRoleFactory : FakeHelperFactory<Role>
{
    public FakeRoleFactory() : base(GetData()) { }

    private static List<Role> GetData()
    {
        return
        [
            new Role(1, "User"),
            new Role(2, "Admin")
        ];
    }
}