using System;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Pagination;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Mock.Factories;

[MockFactoryOf(typeof(Cigar))]
public class MockCigarFactory : MockFactory<Cigar>
{
    public MockCigarFactory() : base(GetData()) { }

    private static Cigar[] GetData()
    {
        Brand[] brands = [new(1, "valami")];

        return Enumerable.Range(0, 125)
            .Select(x => new Cigar(x, Guid.NewGuid().ToString(), brands[0]))
            .ToArray();
    }
}