namespace CigiScraper.Model.Place;

public record PostalLocation(int PostalCode, string City, string County)
{
    public static async Task<PostalLocation[]> ParseFile(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException($"{file} not found");

        var lines = await File.ReadAllLinesAsync(file);

        PostalLocation[] shops = new PostalLocation[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var s = line.Split(',');

            if (string.IsNullOrEmpty(s[2]) && s[1] == "Budapest")
            {
                s[2] = "Pest";
            }

            shops[i] = new PostalLocation(int.Parse(s[0]), s[1], s[2]);
        }

        return shops;
    }
}