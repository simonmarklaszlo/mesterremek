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

        int doubleCount = 0;

        foreach (var shop in shops)
        {
            var byLoc = shop.FindByLocation(unofficialShops);
            var byAdd = shop.FindByAddress(unofficialShops);

            if (byLoc is not null && byAdd is not null)
            {
                doubleCount++;
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

    public static UnofficialShop? FindByLocation(this OfficialShop shop, UnofficialShop[] unofficialShops)
    {
        return unofficialShops.FirstOrDefault(unofficial => unofficial.LocationName == shop.Location.LocationName);
    }

    public static UnofficialShop? FindByAddress(this OfficialShop shop, UnofficialShop[] unofficialShops)
    {
        return unofficialShops.FirstOrDefault(unofficial => unofficial.Address == shop.Location.Address);
    }
}