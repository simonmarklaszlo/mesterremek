using CigiScraper.Model.Place;

namespace CigiScraper.Model.Shop;

public class OfficialShop
{
    private readonly string _rawName;
    private readonly string _rawLocation;
    public string Name => GetCleanName(_rawName);
    public ShopLocation Location => GetLocation(_rawLocation);
    public PostalLocation? PostalLocation { get; set; }


    private OfficialShop(string rawName, string rawLocation)
    {
        _rawName = rawName;
        _rawLocation = rawLocation;
    }



    public override string ToString()
    {
        return $"{Name} - {_rawLocation}";
    }


    public static async Task<OfficialShop[]> ParseFile(string file)
    {
        if(!File.Exists(file))throw new FileNotFoundException($"{file} not found");

        var lines = await File.ReadAllLinesAsync(file);

        OfficialShop[] shops = new OfficialShop[lines.Length];

        for (var i = 0; i < lines.Length; i++)
        {
            var split = lines[i].Replace("\"", "").Split(',');

            int codeIdx = GetCodeIdx(split);

            string name = string.Join(',', split, 0, codeIdx);
            string location = string.Join(',', split, codeIdx + 1, split.Length - (codeIdx + 1) - 2);

            shops[i] = new OfficialShop(name, location);
        }

        return shops;

        static int GetCodeIdx(string[] split)
        {
            for (var i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith("dke", StringComparison.CurrentCultureIgnoreCase)) return i;
            }

            throw new Exception("No code found");
        }
    }
    private static string GetCleanName(string rawName)
    {
        return rawName.Replace("\"","");
    }
    private static ShopLocation GetLocation(string rawLocation)
    {
        int postalCode = -1;
        string locationName = "glorbo";
        string address = "glorbo";

        try
        {
            if (rawLocation.Contains(','))
            {
                var fullSplit = rawLocation.Split(',');
                var firstSplit = fullSplit[0].Split(' ');
                postalCode = int.Parse(firstSplit[0]);
                locationName = firstSplit[1];
                address = fullSplit[1];
            }
            else
            {
                var split = rawLocation.Split(' ');
                postalCode = int.Parse(split[0]);
                locationName = split[1];
                address = string.Join(' ', split, 2, split.Length - 2);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return new ShopLocation(postalCode, locationName, address);
    }
}