using System;
using System.Linq;
using SzivarClubManager.Models;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Mock.Factories;

[MockFactoryOf(typeof(Shop))]
public class MockShopFactory : MockFactory<Shop>
{
    public MockShopFactory() : base(GetData()) { }

    private static Shop[] GetData() =>
        Enumerable.Range(0, 125)
            .Select(x => new Shop(
                x,
                Guid.NewGuid().ToString(),
                "addr",
                "city",
                DateTime.Now.AddDays(-x),
                DateTime.Now.AddDays(x),
                new CustomPgPoint(x / double.Pi, x * double.Pi)))
            .ToArray();
}