using System.Security.Cryptography;
using System.Text;

namespace CigiScraper.LocalData;

public class HtmlCache
{
    private const string DirectoryPath = "./archive/htmlsaves/";
    private readonly string _baseDirPath;
    private readonly string _scrapeName;
    private string ScrapeFileName => _scrapeName.EndsWith(".txt") ? _scrapeName : _scrapeName + ".txt";

    public HtmlCache(string scrapeName, string baseDirPath = DirectoryPath)
    {
        _scrapeName = scrapeName;
        _baseDirPath = baseDirPath;

        Directory.CreateDirectory(_baseDirPath);
    }

    public bool CacheExists()
    {
        return Directory.GetFiles(_baseDirPath).Length > 0;
    }

    public void ClearCache()
    {
        foreach (var file in Directory.GetFiles(_baseDirPath)) File.Delete(file);
    }


    public async Task<string?> GetPage(string url)
    {
        var hash = GetPath(url);
        if (!File.Exists(hash)) return null;

        return await File.ReadAllTextAsync(hash);
    }

    public Task SavePage(string url, string content)
    {
        return File.WriteAllTextAsync(GetPath(url), content);
    }

    public Task SavePageUrls(string[] urls)
    {
        string path = Path.Combine(_baseDirPath, ScrapeFileName);

        return File.WriteAllLinesAsync(path, urls);
    }

    public Task<string[]> GetPageUrls()
    {
        string path = Path.Combine(_baseDirPath, ScrapeFileName);

        if (!File.Exists(path))
        {
            return Task.FromResult(Array.Empty<string>());
        }

        return File.ReadAllLinesAsync(path);
    }

    private string GetPath(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Path.Combine(_baseDirPath, Convert.ToHexString(bytes) + ".html");
    }
}