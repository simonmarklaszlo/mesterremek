using CigiScraper.LocalData;
using CigiScraper.Model.Shop;
using HtmlAgilityPack;

namespace CigiScraper.Scraping;

public abstract class Scraper : IAsyncDisposable
{
    private ScrapeType _scrapeType;
    private bool _allowNetCall;
    private bool _saveToCache;
    protected readonly HtmlCache HtmlCache;
    protected readonly Logger Logger;

    protected Scraper(string scrapeName)
    {
        HtmlCache = new HtmlCache(scrapeName);
        Logger = new Logger(scrapeName);
    }

    public Task<ScrapedShop[]> ScrapeAny(bool forceOnline = false)
    {
        if (forceOnline || !HtmlCache.CacheExists())
        {
            HtmlCache.ClearCache();

            _allowNetCall = true;
            _scrapeType = ScrapeType.Online;
            _saveToCache = true;
            return ScrapeFromOnline();
        }

        _saveToCache = false;
        _allowNetCall = false;
        _scrapeType = ScrapeType.Cache;
        return ScrapeFromCache();
    }

    protected abstract Task<ScrapedShop[]> ScrapeFromOnline();
    protected abstract Task<ScrapedShop[]> ScrapeFromCache();

    protected async Task<HtmlDocument> LoadHtml(string url)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(await GetHtml(url));

        return htmlDocument;
    }

    private async Task<string> GetHtml(string url)
    {
        if (_scrapeType is not ScrapeType.Cache)
        {
            return await PageFromOnline(url);
        }

        var cachePage = await PageFromCache(url);
        if (cachePage is not null) return cachePage;

        if (!_allowNetCall) throw new Exception("Not allowed to make a network call.");

        return await PageFromOnline(url);
    }

    private Task<string?> PageFromCache(string url)
    {
        return HtmlCache.GetPage(url);
    }

    private async Task<string> PageFromOnline(string url)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var htmlString = await response.Content.ReadAsStringAsync();

        if (_saveToCache)
        {
            await HtmlCache.SavePage(url, htmlString);
        }

        return htmlString;
    }

    private enum ScrapeType
    {
        Online,
        Cache,
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Logger.DisposeAsync();
    }
}