using System.Text.RegularExpressions;
using CigiScraper.LocalData;
using CigiScraper.Model.Shop;
using CigiScraper.Model.Time;
using HtmlAgilityPack;

namespace CigiScraper.Scraping;

public partial class Scraper
{
    private const string BaseUrl ="https://nemzetidohanyboltkereso.hu/trafik-lista";
    private bool _throwOnNetCall;

    public Task<UnofficialShop[]> ScrapeAny(bool preferOnline = false)
    {
        if (preferOnline)
        {
            return ScrapeFromOnline();
        }

        return ScrapeFromCache();
    }

    private async Task<UnofficialShop[]> ScrapeFromOnline()
    {
        _throwOnNetCall = false;
        var mainRes = await GetUrlsFromMainPage();

        List<string> urls = [];
        foreach (var url in mainRes)
        {
            urls.AddRange(await GetBoltUrlsFromLocation(url));
        }


        await LocalCache.SavePageUrls(urls.ToArray());

        List<UnofficialShop> bolt = [];
        foreach (var pageUrl in urls)
        {
            bolt.Add(await ParseBoltFromPage(pageUrl));
        }

        return bolt.ToArray();
    }

    private async Task<UnofficialShop[]> ScrapeFromCache()
    {
        _throwOnNetCall = true;

        var urls = await LocalCache.GetPageUrls();
        if (urls.Length == 0)
        {
            try
            {
                var midUrls = await GetUrlsFromMainPage();

                var t1 = midUrls.Select(GetBoltUrlsFromLocation).ToArray();
                var r1 = await Task.WhenAll(t1);
                urls = r1.SelectMany(x => x).ToArray();

                await LocalCache.SavePageUrls(urls);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return [];
            }
        }
        var tasks = urls.Select(ParseBoltFromPage).ToArray();
        var res = await Task.WhenAll(tasks);

        _throwOnNetCall = false;
        return res;
    }

    private async Task<IEnumerable<string>> GetUrlsFromMainPage()
    {
        Console.WriteLine("[Main] Starting");

        var htmlString = await LocalCache.GetPage(BaseUrl) ?? await GetHtml(BaseUrl);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        /*
         <li class="cat-item cat-item-99">
            <a href="https://...">...</a>
        </li>
         */
        var nodes = htmlDocument.DocumentNode.SelectNodes("//li[contains(@class,'cat-item')]/a");


        if (nodes is null)
        {
            Console.WriteLine("[Main] Elements not found");
            Console.WriteLine("[Main] Finished with Error");
            return [];
        }

        var urls = nodes.Select(x => x.GetAttributeValue("href", ""));
        Console.WriteLine("[Main] Finished");
        return urls;
    }

    private async Task<IEnumerable<string>> GetBoltUrlsFromLocation(string url)
    {
        Console.WriteLine($"[Location] Starting : {url}");

        var htmlString = await LocalCache.GetPage(url) ?? await GetHtml(url);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        /*
         <a href="...">
                <svg>...</svg>
                Részletek
            </a>
         */
        var nodes = htmlDocument.DocumentNode.SelectNodes("//a[contains(., 'Részletek')]");
        if (nodes is null)
        {
            Console.WriteLine("[Location] Elements not found");
            Console.WriteLine($"[Location] Finished with Error: {url}");
            return [];
        }

        Console.WriteLine($"[Location] Finished : {url}");
        return nodes.Select(x => x.GetAttributeValue("href", ""));
    }

    private async Task<UnofficialShop> ParseBoltFromPage(string url)
    {
        Console.WriteLine($"[Bolt] Starting : {url}");

        var htmlString = await LocalCache.GetPage(url) ?? await GetHtml(url);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);
        /*
         <dl>
            <dt>Hétfő</dt>
            <dd>6:00-22:00</dd>

            <dt>Kedd</dt>
            <dd>6:00-22:00</dd>

            <dt>Szerda</dt>
            <dd>6:00-22:00</dd>

            <dt>Csütörtök</dt>
            <dd>6:00-22:00</dd>

            <dt>Péntek</dt>
            <dd>6:00-22:00</dd>

            <dt>Szombat</dt>
            <dd>7:00-2:00</dd>

            <dt>Vasárnap</dt>
            <dd>7:00-13:00</dd>
        </dl>
         */

        /*
            <span itemprop="addressLocality">...</span>
         */
        /*
            <span itemprop="streetAddress">...</span>
         */
        HtmlNodeCollection? timeNodes = htmlDocument.DocumentNode.SelectNodes("//dl/*");
        HtmlNode? localityNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='addressLocality']");
        HtmlNode? streetNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='streetAddress']");
        HtmlNode? scriptNode = htmlDocument.DocumentNode.SelectNodes("//script")
            ?.FirstOrDefault(s => s.InnerText.Contains("initSingleTrafikMap"));


        //hely - település + utca
        string city = localityNode?.InnerText.Trim() ?? "";
        var street = streetNode?.InnerText.Trim() ?? "";

        var nyitvatartasok = GetNyitvatartasok(timeNodes, url);
        var (lat, lon) = GetCoordinates(scriptNode);

        Console.WriteLine($"[Bolt] Finished : {url}");
        return new UnofficialShop(url, lon, lat, city, street, nyitvatartasok);
    }

    private static OpeningSchedule[] GetNyitvatartasok(HtmlNodeCollection? nodes, string url)
    {
        if (nodes is null) return [];

        var timeNodesArr = nodes.Where(x => x.Name is "dd" or "dt").ToArray();

        List<OpeningSchedule> nyitvatartasok = [];
        for (int i = 0; i < timeNodesArr.Length; i += 2)
        {
            var day = timeNodesArr[i].InnerText.Trim();
            var hours = timeNodesArr[i + 1].InnerText;


            nyitvatartasok.Add(new OpeningSchedule(day, OpeningHours.Parse(hours, url, nyitvatartasok)));
        }

        return nyitvatartasok.ToArray();
    }

    private static (double, double) GetCoordinates(HtmlNode? script)
    {
        if (script is null) return (double.NaN, double.NaN);

        // Regex -> ... L.marker([lat, lon] ...
        var match = JsCoordArrRegex().Match(script.InnerText);

        if (match.Success) return (double.Parse(match.Groups["lat"].Value), double.Parse(match.Groups["lon"].Value));

        return (double.NaN, double.NaN);
    }
    private async Task<string> GetHtml(string url, bool save = true)
    {
        if (_throwOnNetCall)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[NET CALL] Not allowed");
            throw new Exception("Not allowed to make a network call.");
        }
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[NET CALL] {url}");
        Console.ResetColor();

        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var htmlString = await response.Content.ReadAsStringAsync();

        if (save)
        {
            await LocalCache.SavePage(url, htmlString);
        }

        return htmlString;
    }

    [GeneratedRegex(@"L\.marker\(\s*\[(?<lat>[\d\.]+),\s*(?<lon>[\d\.]+)\]")]
    private static partial Regex JsCoordArrRegex();
}