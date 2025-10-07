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

    public static MixedShop[] MixWith(this OfficialShop[] shops, UnofficialShop[] unofficialShops)
    {
        List<MixedShop> mixed = [];

        foreach (var shop in shops)
        {
            var byLoc = shop.FindByLocation(unofficialShops);
            var byAdd = shop.FindByAddress(unofficialShops);

            if (byLoc is not null && byAdd is not null)
            {
                mixed.Add(new MixedShop(shop,byLoc));
                mixed.Add(new MixedShop(shop,byAdd));
                continue;
            }

            if (byLoc is not null)
            {
                mixed.Add(new MixedShop(shop, byLoc));
            }
            else if (byAdd is not null)
            {
                mixed.Add(new MixedShop(shop, byAdd));
            }
        }

        return mixed.ToArray();
    }

    public static MixedShop[] EliminateDuplicates(this MixedShop[] shops)
    {
        var filtered= shops
            .GroupBy(x => x.Address)
            .Select(x => x.First())
            .ToArray();

        return filtered;
    }

    private static UnofficialShop? FindByLocation(this OfficialShop shop, UnofficialShop[] unofficialShops)
    {
        foreach (var unofficial in unofficialShops)
        {
            if (unofficial.LocationName == shop.Location.LocationName) return unofficial;
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