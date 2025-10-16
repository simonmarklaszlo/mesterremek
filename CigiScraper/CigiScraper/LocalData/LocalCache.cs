using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;

namespace CigiScraper.LocalData;

public static class LocalCache
{
    private const string DirectoryPath = "./archive/htmlsaves/";
    private const string UrlsFName = "urls.txt";
    static LocalCache()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    public static async Task Clean()
    {
        Console.WriteLine("[CACHE] Cleaning");
        if (!Directory.Exists(DirectoryPath))
        {
            Console.WriteLine("[CACHE] Directory does not exist.");
            return;
        }
        var files = Directory.GetFiles(DirectoryPath);

        var tasks = files.Select(DeleteIfIncorrect);
        Console.WriteLine("[CLEAN] Checking");
        var res = await Task.WhenAll(tasks);
        var deleted = res.Count(x => x);
        if (deleted != 0) Console.WriteLine($"[CACHE] Deleted {deleted} incorrect files");
        Console.WriteLine("[CACHE] Cleaned");
    }

    public static void Detete()
    {
        Console.WriteLine("[CACHE] Deleting");

        if (Directory.Exists(DirectoryPath))
        {
            var files = Directory.GetFiles(DirectoryPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        Console.WriteLine("[CACHE] Deleted");
    }

    /// <summary>
    /// Deletes cached page if it's an error page.'
    /// </summary>
    /// <param name="path">Path of the file</param>
    /// <returns>True if the file was deleted</returns>
    private static async Task<bool> DeleteIfIncorrect(string path)
    {
        const int kb = 1024;
        var fileSize = new FileInfo(path).Length;

        if (fileSize > kb*10) return false;

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

    public static Task SavePage(string url, string content)
    {
        var hash = GetPath(url);
        return File.WriteAllTextAsync(hash, content);
    }

    public static Task SavePageUrls(string[] urls)
    {
        var path = Path.Combine(DirectoryPath, UrlsFName);
        return File.WriteAllLinesAsync(path, urls);
    }

    public static Task<string[]> GetPageUrls()
    {
        var path = Path.Combine(DirectoryPath, UrlsFName);
        if (!File.Exists(path)) return Task.FromResult(Array.Empty<string>());
        return File.ReadAllLinesAsync(path);
    }

    private static string GetPath(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Path.Combine(DirectoryPath,Convert.ToHexString(bytes) + ".html");
    }
}