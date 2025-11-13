using CigiScraper.Model.Shop;
using CigiScraper.Model.Time;

namespace CigiScraper.LocalData;

public class Archiver
{
    private readonly string _dirPath;

    public Archiver(string dirPath)
    {
        _dirPath = dirPath;
        Directory.CreateDirectory(dirPath);
    }
    public void ClearFiles()
    {
        foreach (var file in Directory.GetFiles(_dirPath))
        {
            File.Delete(file);
        }
    }

    public async Task ArchiveToCsv(string fileName, UnofficialShop3[] bolts)
    {
        Console.WriteLine($"[ARCHIVER] Writing : {fileName}");
        await using var writer = new StreamWriter(Path.Combine(_dirPath, fileName));
        foreach (var bolt in bolts)
        {
            await writer.WriteLineAsync(bolt.ToCsvLine());
        }

        Console.WriteLine($"[ARCHIVER] Complete: {fileName}");
    }

    public async Task ArchiveCoordsToCsv(string fileName, UnofficialShop3[] bolts)
    {
        Console.WriteLine($"[ARCHIVER] Writing : {fileName}");
        await using var writer = new StreamWriter(Path.Combine(_dirPath, fileName));
        foreach (var bolt in bolts)
        {
            await writer.WriteLineAsync($"{bolt.Longitude};{bolt.Latitude}");
        }

        Console.WriteLine($"[ARCHIVER] Complete: {fileName}");
    }

    public async Task ArchiveToCsv(string fileName, MixedShop[] bolts)
    {
        Console.WriteLine($"[ARCHIVER] Writing : {fileName}");
        await using var writer = new StreamWriter(Path.Combine(_dirPath, fileName));
        foreach (var bolt in bolts)
        {
            await writer.WriteLineAsync(bolt.ToCsvLine());
        }

        Console.WriteLine($"[ARCHIVER] Complete: {fileName}");
    }
    public async Task ArchiveToCsv(string fileName, Shop[] bolts)
    {
        Console.WriteLine($"[ARCHIVER] Writing : {fileName}");
        await using var writer = new StreamWriter(Path.Combine(_dirPath, fileName));
        foreach (var bolt in bolts)
        {
            await writer.WriteLineAsync(bolt.ToCsvLine());
        }

        Console.WriteLine($"[ARCHIVER] Complete: {fileName}");
    }
    public async Task<UnofficialShop3[]> ReadBackFromArchive(string fileName)
    {
        var path = Path.Combine(_dirPath, fileName);
        if (!File.Exists(path)) return [];

        var lines = await File.ReadAllLinesAsync(path);
        UnofficialShop3[] bolts = new UnofficialShop3[lines.Length];

        for (var i = 0; i < lines.Length; i++)
        {
            var split =  lines[i].Split(';');

            var lonSucc = double.TryParse(split[0], out var lon);
            var latSucc = double.TryParse(split[1], out var lat);
            double longitude = lonSucc ? lon : double.NaN;
            double latitude = latSucc ? lat : double.NaN;
            string location = split[2];
            string street = split[3];
            OpeningSchedule[] nyitvatartasok = ParseNyitvatartasok(split[4]);
            bolts[i] = new UnofficialShop3("", longitude, latitude, location, street, nyitvatartasok);
        }

        return bolts;

        OpeningSchedule[] ParseNyitvatartasok(string str)
        {
            if (string.IsNullOrEmpty(str)) return [];

            var split = str.Split('|');
            if (split.Length == 0) return [];
            List<OpeningSchedule> ny = [];
            foreach (var day in split)
            {
                var s = day.Split(' ');
                string dayName = s[0];
                OpeningHours? nyitvatartas = null;

                if (s.Length == 2)
                {
                    var hoursSplit = s[1].Split('-');

                    if (hoursSplit.Length == 2)
                    {
                        nyitvatartas = new OpeningHours(TimeOnly.Parse(hoursSplit[0]), TimeOnly.Parse(hoursSplit[1]));
                    }
                }

                ny.Add(new OpeningSchedule(dayName, nyitvatartas));
            }

            return ny.ToArray();
        }
    }
}