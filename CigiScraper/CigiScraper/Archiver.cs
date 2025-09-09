using CigiScraper.Model;

namespace CigiScraper;

public class Archiver
{
    private readonly string _dirPath;

    public Archiver(string dirPath, bool deleteExisting = true)
    {
        _dirPath = dirPath;
        Directory.CreateDirectory(dirPath);

        if (!deleteExisting) return;

        foreach (var file in Directory.GetFiles(_dirPath))
        {
            File.Delete(file);
        }
    }
    public async Task ArchiveToCsv(string fileName, Bolt[] bolts)
    {
        Console.WriteLine($"[ARCHIVER] Writing : {fileName}");
        await using var writer = new StreamWriter(Path.Combine(_dirPath, fileName));
        foreach (var bolt in bolts)
        {
            await writer.WriteLineAsync(bolt.ToCsvLine());
        }
        Console.WriteLine($"[ARCHIVER] Writing complete: {fileName}");
    }
}