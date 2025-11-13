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


    public static Shop[] FindWithMatchingAddress(this UnofficialShop3[] unofficialShops, OfficialShop[] shops)
    {
        List<Shop> resShop = [];

        foreach (var us in unofficialShops)
        {
            var byAddress = us.FindByAddress(shops);
            if(byAddress is not null) resShop.Add(new Shop(byAddress.Name, us.City, us.Address, us.OpeningSchedules , us.Longitude, us.Latitude));
        }


        return resShop.ToArray();
    }
    public static UnofficialShop3[] EliminateDuplicates(this UnofficialShop3[] shops)
    {
        UnofficialShop3?[] resNullable = shops.Select(UnofficialShop3? (x) => x).ToArray();
        for (var i = 0; i < resNullable.Length; i++)
        {
            for (var j = 0; j < resNullable.Length; j++)
            {
                if (i == j) continue;
                bool equal = resNullable[i]?.Equals(resNullable[j]) ?? false;
                if (equal) resNullable[i] = null;
            }
        }

        return resNullable.OfType<UnofficialShop3>().ToArray();
    }


    private static OfficialShop? FindByAddress(this UnofficialShop3 shop3, OfficialShop[] officialShops)
    {
        foreach (var officialShop in officialShops)
        {
            if (officialShop.Location.Address == shop3.Address) return officialShop;
        }

        return null;
    }
}