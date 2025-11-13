using System.Text.RegularExpressions;
using CigiScraper.LocalData;
using CigiScraper.Model.Shop;
using CigiScraper.Model.Time;
using HtmlAgilityPack;

namespace CigiScraper.Scraping;

public partial class NdbScraper() : Scraper("Ndb")
{
    private const string BaseUrl = "https://nemzetidohanyboltkereso.hu/trafik-lista";

    protected override async Task<ScrapedShop[]> ScrapeFromOnline()
    {
        var places = await GetPlaceUrls();
        IEnumerable<string> shopUrlsEnum = [];

        foreach (var place in places)
        {
            var urls = await GetShopUrlsFromPlace(place);
            shopUrlsEnum = shopUrlsEnum.Concat(urls);
        }

        var shopUrls = shopUrlsEnum.ToArray();

        await HtmlCache.SavePageUrls(shopUrls);

        ScrapedShop[] shops = new ScrapedShop[shopUrls.Length];

        for (var i = 0; i < shopUrls.Length; i++)
        {
            shops[i] = await ParseShopFromPage(shopUrls[i]);
        }

        return shops;
    }

    protected override async Task<ScrapedShop[]> ScrapeFromCache()
    {
        var shopUrls = await HtmlCache.GetPageUrls();
        ScrapedShop[] shops = new ScrapedShop[shopUrls.Length];
        int index = 0;

        await foreach (var task in Task.WhenEach(shopUrls.Select(ParseShopFromPage)))
        {
            shops[index++] = await task;
        }

        return shops.ToArray();
    }

    private async Task<IEnumerable<string>> GetPlaceUrls()
    {
        var htmlDocument = await LoadHtml(BaseUrl);

        var nodes = htmlDocument.DocumentNode
            .SelectNodes("//li[contains(@class,'cat-item')]/a");


        if (nodes is null)
        {
            const string msg = $"Unable to find urls on {BaseUrl}";
            await Logger.Error(msg);
            throw new Exception(msg);
        }

        return nodes.Select(x => x.GetAttributeValue("href", ""));
    }

    private async Task<IEnumerable<string>> GetShopUrlsFromPlace(string url)
    {
        var htmlDocument = await LoadHtml(url);

        var nodes = htmlDocument.DocumentNode.SelectNodes("//a[contains(., 'Részletek')]");

        if (nodes is null)
        {
            string msg = $"Unable to find shop urls on {url}";
            await Logger.Error(msg);
            throw new Exception(msg);
        }

        return nodes.Select(x => x.GetAttributeValue("href", ""));
    }

    private async Task<ScrapedShop> ParseShopFromPage(string url)
    {
        var htmlDocument = await LoadHtml(url);

        HtmlNodeCollection? timeNodes = htmlDocument.DocumentNode.SelectNodes("//dl/*");
        HtmlNode? localityNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='addressLocality']");
        HtmlNode? streetNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='streetAddress']");
        HtmlNode? scriptNode = htmlDocument.DocumentNode.SelectNodes("//script")
            ?.FirstOrDefault(s => s.InnerText.Contains("initSingleTrafikMap"));


        string city = localityNode?.InnerText.Trim() ?? string.Empty;
        var street = streetNode?.InnerText.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(city))
        {
            await Logger.Warn($"No city found for {url}");
        }
        if (string.IsNullOrEmpty(street))
        {
            await Logger.Warn($"No street found for {url}");
        }


        var openingSchedule = ExtractOpeningSchedule(timeNodes, url);
        if (openingSchedule.Length == 0)
        {
            await Logger.Warn($"No opening schedule found for {url}");
        }


        var (lat, lon) = ExtractCoordinates(scriptNode);
        if (double.IsNaN(lat) || double.IsNaN(lon))
        {
            await Logger.Warn($"No coordinates found for {url}");
        }

        return new ScrapedShop(url,null, city, street, openingSchedule, lat, lon);
    }

    private OpeningSchedule[] ExtractOpeningSchedule(HtmlNodeCollection? nodes, string url)
    {
        if (nodes is null) return [];

        var timeNodesArr = nodes.Where(x => x.Name is "dd" or "dt").ToArray();

        List<OpeningSchedule> nyitvatartasok = [];
        for (int i = 0; i < timeNodesArr.Length; i += 2)
        {
            var day = timeNodesArr[i].InnerText.Trim();
            var hours = timeNodesArr[i + 1].InnerText;


            nyitvatartasok.Add(new OpeningSchedule(day, OpeningHours.Parse(hours, url, nyitvatartasok, Logger)));
        }

        return nyitvatartasok.ToArray();
    }

    private static (double, double) ExtractCoordinates(HtmlNode? script)
    {
        if (script is null) return (double.NaN, double.NaN);

        var match = RegexJsCoordArray().Match(script.InnerText);

        if (match.Success) return (double.Parse(match.Groups["lat"].Value), double.Parse(match.Groups["lon"].Value));

        return (double.NaN, double.NaN);
    }

    [GeneratedRegex(@"L\.marker\(\s*\[(?<lat>[\d\.]+),\s*(?<lon>[\d\.]+)\]")]
    private static partial Regex RegexJsCoordArray();
}