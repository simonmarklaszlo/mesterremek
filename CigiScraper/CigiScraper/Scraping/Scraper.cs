using CigiScraper.LocalData;
using CigiScraper.Model.Shop;
using HtmlAgilityPack;

namespace CigiScraper.Scraping;

public abstract class Scraper : IAsyncDisposable
{
    private ScrapeType _scrapeType;
    private bool _allowNetCall;
    private bool _saveToCache;
    private readonly HtmlCache _htmlCache;
    protected readonly Logger Logger;
    protected Scraper(string scrapeName)
    {
        _htmlCache = new HtmlCache(scrapeName);
        Logger = new Logger(scrapeName);
    }

    public Task<UnofficialShop[]> ScrapeAny(bool preferOnline = false)
    {
        _saveToCache = true;
        if (preferOnline)
        {
            _allowNetCall = true;
            _scrapeType = ScrapeType.Online;
            return ScrapeFromOnline();
        }

        _scrapeType = ScrapeType.Cache;
        return ScrapeFromCache();
    }

    protected abstract Task<UnofficialShop[]> ScrapeFromOnline();
    protected abstract Task<UnofficialShop[]> ScrapeFromCache();

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
        return _htmlCache.GetPage(url);
    }

    private async Task<string> PageFromOnline(string url)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var htmlString = await response.Content.ReadAsStringAsync();

        if (_saveToCache)
        {
            await _htmlCache.SavePage(url, htmlString);
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