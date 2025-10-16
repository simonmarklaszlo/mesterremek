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


    public static Shop[] FindWithMatchingAddress(this UnofficialShop[] unofficialShops, OfficialShop[] shops)
    {
        List<Shop> resShop = [];

        foreach (var us in unofficialShops)
        {
            var byAddress = us.FindByAddress(shops);
            if(byAddress is not null) resShop.Add(new Shop(byAddress.Name, us.City, us.Address, us.OpeningSchedules , us.Longitude, us.Latitude));
        }


        return resShop.ToArray();
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


    private static OfficialShop? FindByAddress(this UnofficialShop shop, OfficialShop[] officialShops)
    {
        foreach (var officialShop in officialShops)
        {
            if (officialShop.Location.Address == shop.Address) return officialShop;
        }

        return null;
    }
}