using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;

namespace CigiScraper;

public static class LocalCache
{
    private const string DirectoryPath = "./saves/";

    static LocalCache()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    public static async Task Clean()
    {
        var files = Directory.GetFiles(DirectoryPath);

        var tasks = files.Select(DeleteIfIncorrect);
        var res = await Task.WhenAll(tasks);
        var deleted = res.Count(x => x);
        Console.WriteLine($"[CACHE] Deleted {deleted} incorrect files");
    }

    /// <summary>
    /// Deletes cached page if it's an error page.'
    /// </summary>
    /// <param name="path">Path of the file</param>
    /// <returns>True if the file was deleted</returns>
    private static async Task<bool> DeleteIfIncorrect(string path)
    {
        var htmlString = await File.ReadAllTextAsync(path);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        HtmlNode? body = htmlDocument.DocumentNode.SelectSingleNode("//body[@id='error-page']");

        if (body is null) return false;

        File.Delete(path);
        Console.WriteLine($"[CACHE] Deleted incorrect file: {path}");
        return true;
    }

    public static async Task<string?> GetPage(string url)
    {
        var hash = GetPath(url);
        if (!File.Exists(hash)) return null;

        return await File.ReadAllTextAsync(hash);
    }

    public static async Task SavePage(string url, string content)
    {
        var hash = GetPath(url);
        await File.WriteAllTextAsync(hash, content);
    }

    private static string GetPath(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Path.Combine(DirectoryPath,Convert.ToHexString(bytes) + ".html");
    }
}