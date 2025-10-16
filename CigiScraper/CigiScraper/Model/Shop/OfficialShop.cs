using CigiScraper.Model.Place;

namespace CigiScraper.Model.Shop;

public class OfficialShop
{
    private readonly string _rawName;
    private readonly string _rawLocation;

    public string Name
    {
        get
        {
            if(_name is not null)return _name;
            _name = GetCleanName(_rawName);
            return _name;
        }
    }
    private string? _name;

    public ShopLocation Location
    {
        get
        {
            if (_location is not null) return _location;
            _location = GetLocation(_rawLocation);
            return _location;
        }
    }
    private ShopLocation? _location;
    public PostalLocation? PostalLocation { get; set; }
    private static readonly char[] AddressSeparators = [',', ' ' ];


    private OfficialShop(string rawName, string rawLocation)
    {
        _rawName = rawName;
        _rawLocation = rawLocation;
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
        var parts = rawLocation
            .Split(AddressSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToArray();

        var postalCode = int.Parse(parts[0]);
        var locationName = parts[1].Replace("u.", "utca");
        var address = string.Join(' ', parts, 2, parts.Length - 2);

        return new ShopLocation(postalCode, locationName, address);
    }
}