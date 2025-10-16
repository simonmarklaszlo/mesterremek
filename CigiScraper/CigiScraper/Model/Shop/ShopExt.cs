using CigiScraper.Model.Place;

namespace CigiScraper.Model.Shop;

public static class ShopExt
{
    public static void MapTo(this OfficialShop[] shops, PostalLocation[] postalLocations)
    {
        foreach (var shop in shops)
        {
            shop.PostalLocation = postalLocations.FirstOrDefault(x => x.PostalCode == shop.Location.PostalCode);
        }
    }

    public static MixedShop[] FindExactMatches(this OfficialShop[] shops, UnofficialShop[] unofficialShops)
    {
        List<MixedShop> mixed = [];

        foreach (var shop in shops)
        {
            var byAdd = shop.FindByAddress(unofficialShops);

            if (byAdd is not null && byAdd.City == shop.Location.City) mixed.Add(new MixedShop(shop, byAdd));
        }

        return mixed.EliminateDuplicates();
    }
    public static MixedShop[] MixWith(this OfficialShop[] shops, UnofficialShop[] unofficialShops, MixedShop[]? exclude = null)
    {
        List<MixedShop> mixed = [];

        foreach (var shop in shops)
        {
            if (exclude?.Any(x => x.Address == shop.Location.Address) ?? false) continue;

            var byLoc = shop.FindByLocation(unofficialShops);
            var byAdd = shop.FindByAddress(unofficialShops);

            if (byAdd is not null && byAdd.City == shop.Location.City) mixed.Add(new MixedShop(shop, byAdd));
            if (byLoc is not null) mixed.Add(new MixedShop(shop, byLoc));
        }

        return mixed.EliminateDuplicates();
    }
    public static MixedShop[] MixOld(this OfficialShop[] shops, UnofficialShop[] unofficialShops)
    {
        List<MixedShop> mixed = [];

        foreach (var shop in shops)
        {
            var byLoc = shop.FindByLocation(unofficialShops);
            var byAdd = shop.FindByAddress(unofficialShops);

            if (byAdd is not null) mixed.Add(new MixedShop(shop, byAdd));
            if (byLoc is not null) mixed.Add(new MixedShop(shop, byLoc));
        }

        return mixed.EliminateDuplicates();
    }


    public static MixedShop[] EliminateDuplicates(this IEnumerable<MixedShop> shops)
    {
        var filtered= shops
            .GroupBy(x => x.Address)
            .Select(x => x.First())
            .ToArray();

        return filtered;
    }
    public static UnofficialShop[] EliminateDuplicates(this UnofficialShop[] shops)
    {
        UnofficialShop?[] resNullable = shops.Select(UnofficialShop? (x) => x).ToArray();
        for (var i = 0; i < resNullable.Length; i++)
        {
            for (var j = 0; j < resNullable.Length; j++)
            {
                if (i == j) continue;
                bool equal = resNullable[i]?.Equals(resNullable[j]) ?? false;
                if (equal) resNullable[i] = null;
            }
        }

        return resNullable.OfType<UnofficialShop>().ToArray();
    }


    private static UnofficialShop? FindByLocation(this OfficialShop shop, UnofficialShop[] unofficialShops)
    {
        foreach (var unofficial in unofficialShops)
        {
            if (unofficial.City == shop.Location.City) return unofficial;
        }

        return null;
    }
    private static UnofficialShop? FindByAddress(this OfficialShop shop, UnofficialShop[] unofficialShops)
    {
        foreach (var unofficial in unofficialShops)
        {
            if (unofficial.Address == shop.Location.Address) return unofficial;
        }

        return null;
    }
}