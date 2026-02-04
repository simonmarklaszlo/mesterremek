using Bogus;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Fake.Factories;

[FakeFactoryOf(typeof(Cigar))]
public class FakeCigarFactory : FakeFactory<Cigar>
{
    public FakeCigarFactory() : base(GetData()) { }

    private static Cigar[] GetData()
    {
        var brandFaker = new Faker<Brand>()
            .CustomInstantiator(f =>
                new Brand(
                    f.IndexFaker + 1,
                    f.Company.CompanyName()
                ));

        var brands = brandFaker.Generate(5);

        var cigarFaker = new Faker<Cigar>()
            .CustomInstantiator(f =>
                new Cigar(
                    f.IndexFaker,
                    f.Commerce.ProductName(),
                    f.PickRandom(brands)
                ));

        return cigarFaker.Generate(128).ToArray();
    }
}