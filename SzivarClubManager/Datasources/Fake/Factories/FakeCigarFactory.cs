using System.Collections.Generic;
using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(Cigar), typeof(FakeBrandFactory))]
public sealed class FakeCigarFactory : FakePageFactory<Cigar>
{
    public FakeCigarFactory(FakeBrandFactory brandFactory) : base(GetData(brandFactory)) { }

    private static List<Cigar> GetData(FakeBrandFactory brandFactory)
    {
        Brand[] brands = brandFactory.GetAll().Result;

        var cigarFaker = new Faker<Cigar>()
            .CustomInstantiator(f =>
                new Cigar(
                    f.IndexFaker,
                    f.Commerce.ProductName(),
                    f.PickRandom(brands)
                ));

        return cigarFaker.Generate(128);
    }
}