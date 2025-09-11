using System.Text.RegularExpressions;
using CigiScraper.Model;
using HtmlAgilityPack;

namespace CigiScraper;

public partial class Scraper
{
    public const string BaseUrl = "https://nemzetidohanyboltkereso.hu/trafik-lista";
    private readonly string _baseUrl;
    private bool _throwOnNetCall;
    public Scraper(string baseUrl = BaseUrl)
    {
        _baseUrl = baseUrl;
    }

    public async Task<Bolt[]> ScrapeFullHybrid(string[]? pageUrls = null)
    {
        _throwOnNetCall = false;
        var mainRes = await GetUrlsFromMainPage();

        List<string> urls = [];

        if (pageUrls is null)
        {
            foreach (var url in mainRes)
            {
                urls.AddRange(await GetBoltUrlsFromLocation(url));
            }
        }
        else
        {
            urls = pageUrls.ToList();
        }

        await LocalCache.SavePageUrls(urls.ToArray());

        List<Bolt> bolt = [];
        foreach (var pageUrl in urls)
        {
            bolt.Add(await ParseBoltFromPage(pageUrl));
        }

        return bolt.ToArray();
    }

    public async Task<Bolt[]> ScrapeFromCache()
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

    /// <summary>
    /// Attempts to repair errors in the provided array by re-parsing.
    /// Returns the count of successfully repaired bolts and the count of remaining unrepaired errors after the process.
    /// </summary>
    /// <param name="bolts">An array of <see cref="Bolt"/> objects, potentially containing errors that need to be fixed.</param>
    /// <returns>A tuple containing the count of fixed errors and the count of remaining unrepaired errors.</returns>
    public async Task<(int fixedCount, int remainingErrors)> RepairErrors(Bolt[] bolts)
    {
        await LocalCache.Clean();

        int errorCount = 0;
        int fixedCount = 0;
        for (var i = 0; i < bolts.Length; i++)
        {
            var b = bolts[i];
            if(!b.HasError)continue;

            bolts[i] = await ParseBoltFromPage(b.Url);

            errorCount++;
            if (!bolts[i].HasError)fixedCount++;
        }

        return (fixedCount,errorCount-fixedCount);
    }

    public async Task<(int fixedCount, int remainingErrors)> RepairErrorsWithRetries(Bolt[] bolts, int retries = 1)
    {
        if (retries < 1) retries = 1;
        int initialErrorCount = -1;
        int totalFixedCount = 0;

        while (retries > 0)
        {
            int errorCount = 0;
            int fixedCount = 0;
            for (var i = 0; i < bolts.Length; i++)
            {
                var b = bolts[i];
                if(!b.HasError)continue;

                bolts[i] = await ParseBoltFromPage(b.Url);

                errorCount++;
                if (!bolts[i].HasError)fixedCount++;
            }

            if(initialErrorCount == -1) initialErrorCount = errorCount;
            totalFixedCount += fixedCount;
            retries--;
        }

        if (initialErrorCount == -1) initialErrorCount = 0;

        return (totalFixedCount,initialErrorCount-totalFixedCount);
    }

    private async Task<IEnumerable<string>> GetUrlsFromMainPage()
    {
        Console.WriteLine("[Main] Starting");

        var htmlString = await LocalCache.GetPage(_baseUrl) ?? await GetHtml(_baseUrl);
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

    private async Task<Bolt> ParseBoltFromPage(string url)
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
        string locality = localityNode?.InnerText.Trim() ?? "";
        var street = streetNode?.InnerText.Trim() ?? "";

        var nyitvatartasok = GetNyitvatartasok(timeNodes);
        var (lat, lon) = GetCoordinates(scriptNode);

        Console.WriteLine($"[Bolt] Finished : {url}");
        return new Bolt(url, lon, lat, locality, street, nyitvatartasok);;
    }

    private static Nyitvatartas[] GetNyitvatartasok(HtmlNodeCollection? nodes)
    {
        if (nodes is null) return [];

        var timeNodesArr = nodes.Where(x => x.Name is "dd" or "dt").ToArray();

        List<Nyitvatartas> nyitvatartasok = [];
        for (int i = 0; i < timeNodesArr.Length; i += 2)
        {
            var day = timeNodesArr[i].InnerText.Trim();
            var hours = timeNodesArr[i + 1].InnerText.Trim().Split('-');


            if (hours.Length == 2)
            {
                //vhol 5.00-22.00
                hours[0] = hours[0].Replace('.',':');
                hours[1] = hours[1].Replace('.',':');

                var ido = new Idotartam(hours[0], hours[1]);
                nyitvatartasok.Add(new Nyitvatartas(day,ido));
            }
            else
            {
                nyitvatartasok.Add(new Nyitvatartas(day));
            }
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