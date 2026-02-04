using System;
using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(Shop))]
public class FakeShopFactory : FakeFactory<Shop>
{
    public FakeShopFactory() : base(GetData()) { }

    private static Shop[] GetData()
    {
        var shopFaker = new Faker<Shop>()
            .CustomInstantiator(f =>
            {
                var created = f.Date.Past(3);

                return new Shop(
                    f.IndexFaker,
                    f.Company.CompanyName(),
                    f.Address.StreetAddress(),
                    f.Address.City(),
                    created,
                    f.Date.Between(created, DateTime.Now),
                    new CustomPgPoint(
                        f.Address.Longitude(),
                        f.Address.Latitude()
                    )
                );
            });

        return shopFaker.Generate(128).ToArray();
    }
}