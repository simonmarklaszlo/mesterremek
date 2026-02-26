using System.Collections.Generic;
using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(User), typeof(FakeRoleFactory))]
public class FakeUserFactory : FakePageFactory<User>
{
    public FakeUserFactory(FakeRoleFactory roleFactory) : base(GetData(roleFactory)) { }

    private static List<User> GetData(FakeRoleFactory roleFactory)
    {
        Role[] roles = roleFactory.GetAll().Result;

        var userFaker = new Faker<User>()
            .CustomInstantiator(f =>
                new User(
                    f.IndexFaker,
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Date.Past(2),
                    f.PickRandom(roles)
                ));

        return userFaker.Generate(128);
    }
}