using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Playwright;

namespace CigiScraper.Scraping.InBrowser;

public partial class CylexLoader : SiteLoader
{
    private const string Name = "Cylex";
    private const string BaseUrl = "https://www.cylex.hu/";

    static CylexLoader() => ValidateXpaths();

    private CylexLoader(IBrowserContext context, IPage page) : base(Name, context, page)
    {
    }

    public override Task Load() => NavigateToPlaces();

    private async Task NavigateToPlaces()
    {
        if (Page.Url != BaseUrl)
        {
            string msg = $"Not on the right page {Page.Url} expected {BaseUrl}";
            await Logger.Error(msg);
            throw new Exception(msg);
        }

        var searchInput = Page.Locator(XpathMainSearchInput);
        await searchInput.FillAsync("trafik");
        await searchInput.PressAsync("Enter");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var linkA = Page.Locator(XPathGroupAAnchor);
        await linkA.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await NavigateToEachPlace();
    }

    private async Task NavigateToEachPlace()
    {
        if (Page.Url != "https://www.cylex.hu/trafik/helyek/A")
        {
            string msg = $"Not on the right page {Page.Url} expected https://www.cylex.hu/trafik/helyek/A";
            await Logger.Error(msg);
            throw new Exception(msg);
        }

        char[] placeGroupsToVisit = ExtractPlaceGroups(await Page.ContentAsync());
        var rangeRegex = RegexShopRange();

        foreach (var group in placeGroupsToVisit)
        {
            await DownloadCurrentPage();
            var placeNames = ExtractPlaceNames((await HtmlCache.GetPage(Page.Url))!);
            foreach (var name in placeNames)
            {
                if (name != "Aba") return;

                var placeAnchor = Page.Locator(XPathPlaceAnchor(name));
                await placeAnchor.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await NavigateToEachShop(rangeRegex);
            }
        }
    }

    private async Task NavigateToEachShop(Regex rangeRegex)
    {
        await DownloadCurrentPage();

        string currentHtml = (await HtmlCache.GetPage(Page.Url))!;
        var (start, end) = ExtractShopRange(currentHtml, rangeRegex);

        for (int i = start; i < end; i++)
        {
            var shopPageAnchor = Page.Locator(XPathShopPageAnchor(i + 1));
            await shopPageAnchor.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var acceptBtn = Page.Locator(XPathAgeVerificationButton);
            if (await acceptBtn.CountAsync() > 0 && await acceptBtn.IsVisibleAsync())
            {
                await acceptBtn.ClickAsync();
            }

            await DownloadCurrentPage();

            await Page.GoBackAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        if (end % 20 == 0)
        {
            var paginationNumber = ExtractCurrentPaginationPage(currentHtml);
            var nextPageBtn = Page.Locator($"//nav//ul[contains(@class,'pagination')]//li[contains(@class,'page-item')]//a[text()='{paginationNumber + 1}']");

            if (await nextPageBtn.CountAsync() > 0)
            {
                await nextPageBtn.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await NavigateToEachShop(rangeRegex);
            }
        }

        await Page.GoBackAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private char[] ExtractPlaceGroups(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var groups = doc.DocumentNode
            .SelectNodes(XPathGroupSpans)
            ?.Select(x => x.InnerText.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x =>
            {
                if (x.Length != 1) Logger.Error($"Place group {x} is longer than 1 character.");
                return x[0];
            })
            .ToArray();

        if (groups is null)
        {
            string msg = $"No place groups found for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        return groups;
    }

    private string[] ExtractPlaceNames(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var names = doc.DocumentNode
            .SelectNodes(XPathPlaceAnchors)
            ?.Select(x => x.InnerText.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        if (names is null)
        {
            string msg = $"No place names found for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        return names;
    }

    private (int Start, int End) ExtractShopRange(string html, Regex rangeRegex)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var str = doc.DocumentNode
            .SelectSingleNode(XPathShopRangeSpan)
            ?.InnerText.Trim();

        if (str is null)
        {
            string msg = $"No shop range found for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        var match = rangeRegex.Match(str);
        if (!match.Success)
        {
            string msg = $"No shop range found in {str} for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    private int ExtractCurrentPaginationPage(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var current = doc.DocumentNode
            .SelectSingleNode(XPathPaginationAnchor)
            ?.InnerText.Trim();

        if (current is null)
        {
            string msg = $"No pagination index found for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        if (!int.TryParse(current, out var i))
        {
            string msg = $"Pagination index {current} is not a number for {Page.Url}";
            Logger.Error(msg);
            throw new Exception(msg);
        }

        return i;
    }

    public static async Task<CylexLoader> CreateNew(IPlaywright playwright)
    {
        var userDataDir = Path.Combine(ArchiveBase, Name);
        var context = await playwright.Chromium.LaunchPersistentContextAsync(userDataDir,
            new BrowserTypeLaunchPersistentContextOptions
            {
                Channel = "chrome",
                Headless = false,
                Locale = "en-US",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                            "AppleWebKit/537.36 (KHTML, like Gecko) " +
                            "Chrome/120.0.0.0 Safari/537.36"
            });

        var page = await context.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var doNotConsentButton = page.Locator("//button[contains(@class,'fc-button') and contains(@class,'fc-cta-do-not-consent') and contains(@class,'fc-secondary-button')]");
        if (await doNotConsentButton.IsVisibleAsync())
        {
            await doNotConsentButton.ClickAsync();
        }

        return new CylexLoader(context, page);
    }


    [GeneratedRegex(@"\(([0-9]+)\s-\s([0-9]+)\stalálat\)")]
    private static partial Regex RegexShopRange();


    private const string XpathMainSearchInput = "//input[@id='search-what']";
    private const string XPathGroupAAnchor = "//a[contains(@href,'/trafik/helyek/A') and normalize-space(text())='A']";
    private const string XPathAgeVerificationButton = "//div[@id='age-verification']//button[contains(@class,'yes-btn') and @onclick=\"$('#adultContentModal').modal('hide')\"]";
    private const string XPathGroupSpans = "//div[contains(@class,'azlist')]//ul//li//span";
    private const string XPathPlaceAnchors = "//div//ul[contains(@class,'row') and contains(@class,'list-unstyled')]//li//a";
    private const string XPathShopRangeSpan = "//div[contains(@class,'lm-h')]//h2[contains(@class,'d-inline-block') and contains(@class,'mt-0')]//span[contains(@class,'bold') and contains(@class,'text-muted')]";
    private const string XPathPaginationAnchor = "//nav//ul[contains(@class,'pagination')]//li[contains(@class,'page-item') and contains(@class,'active')]//a[not(@href)]";
    private static string XPathPlaceAnchor(string placeName) => $"//div//ul[contains(@class,'row') and contains(@class,'list-unstyled')]//li//a[@title='{placeName}']";
    private static string XPathShopPageAnchor(int shopPage) => $"(//div[@id='company-container']//div[contains(@class,'lm-comp')])[{shopPage}]";

    private static void ValidateXpaths()
    {
        ValidateXpath(XpathMainSearchInput);
        ValidateXpath(XPathGroupAAnchor);
        ValidateXpath(XPathAgeVerificationButton);
        ValidateXpath(XPathGroupSpans);
        ValidateXpath(XPathPlaceAnchors);
        ValidateXpath(XPathShopRangeSpan);
        ValidateXpath(XPathPaginationAnchor);
        ValidateXpath(XPathPlaceAnchor("Aba"));
        ValidateXpath(XPathShopPageAnchor(1));
    }
}