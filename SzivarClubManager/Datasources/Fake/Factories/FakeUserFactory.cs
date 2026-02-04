using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(User))]
public class FakeUserFactory : FakeFactory<User>
{
    public FakeUserFactory() : base(GetData()) { }

    private static User[] GetData()
    {
        var roles = new[]
        {
            new Role(1, "Admin"),
            new Role(2, "User")
        };

        var userFaker = new Faker<User>()
            .CustomInstantiator(f =>
                new User(
                    f.IndexFaker,
                    f.Name.FullName(),
                    f.Internet.Email(),
                    f.Date.Past(2),
                    f.PickRandom(roles)
                ));

        return userFaker.Generate(128).ToArray();
    }
}