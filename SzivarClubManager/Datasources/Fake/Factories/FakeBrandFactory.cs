using System.Collections.Generic;
using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(Brand))]
public sealed class FakeBrandFactory : FakeHelperFactory<Brand>
{
    public FakeBrandFactory() : base(GetData()) { }

    private static List<Brand> GetData()
    {
        var brandFaker = new Faker<Brand>()
            .CustomInstantiator(f =>
                new Brand(
                    f.IndexFaker + 1,
                    f.Company.CompanyName()
                ));

        return brandFaker.Generate(25);
    }
}