using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CigiScraper;

public partial class Scraper
{
    private readonly string _baseUrl;
    public Scraper(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public async Task<Bolt[]> Run()
    {
        var mainRes = await GetUrlsFromMainPage();
        //  var tasks = mainRes.Select(GetBoltsFromLocation);
        //
        //  var bolts = await Task.WhenAll(tasks);
        //
        // return bolts.SelectMany(x => x).ToArray();

        List<Bolt> bolts = new List<Bolt>();
        foreach (var url in mainRes)
        {
            var res = (await GetBoltsFromLocation(url)).ToArray();
            bolts.AddRange(res);
        }
        return bolts.ToArray();
    }

    private async Task<IEnumerable<string>> GetUrlsFromMainPage()
    {
        Console.WriteLine("[Main] Starting");
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(_baseUrl);
        var htmlString = await response.Content.ReadAsStringAsync();

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
            Console.WriteLine("[Main] Finished");
            Console.WriteLine("[Main] Elements not found");
            return [];
        }

        var urls = nodes.Select(x => x.GetAttributeValue("href", ""));
        Console.WriteLine("[Main] Finished");
        return urls;
    }

    private static async Task<IEnumerable<Bolt>> GetBoltsFromLocation(string url)
    {
        Console.WriteLine($"[Location] Starting : {url}");
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var htmlString = await response.Content.ReadAsStringAsync();

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
            Console.WriteLine($"[Location] Finished with Error: {url}");
            Console.WriteLine("[Location] Elements not found");
            return [];
        }

        var urls = nodes.Select(x => x.GetAttributeValue("href", ""));
        var tasks = urls.Select(ParseBoltFromPage);
        var res = await Task.WhenAll(tasks);
        Console.WriteLine($"[Location] Finished : {url}");
        return res;
    }

    private static async Task<Bolt> ParseBoltFromPage(string url)
    {
        Console.WriteLine($"[Bolt] Starting : {url}");
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        var htmlString = await response.Content.ReadAsStringAsync();
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
        var timeNodes = htmlDocument.DocumentNode.SelectNodes("//dl/*");
        var localityNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='addressLocality']");
        var streetNode   = htmlDocument.DocumentNode.SelectSingleNode("//span[@itemprop='streetAddress']");
        var scriptNode = htmlDocument.DocumentNode.SelectNodes("//script")?.FirstOrDefault(s => s.InnerText.Contains("initSingleTrafikMap"));

        if (timeNodes is null || localityNode is null || streetNode is null || scriptNode is null)
        {
            Console.WriteLine($"[Bolt] Finished with Error: {url}");
            Console.WriteLine("[Bolt] Elements not found");
            return null;
        }

        //hely - település + utca
        var locality = localityNode.InnerText.Trim();
        var street = streetNode.InnerText.Trim();

        //hely - koordináta
        double lat;
        double lon;
        string script = scriptNode.InnerText;

        // Regex to capture [lat, lon]
        var match = JsCoordArrRegex().Match(script);

        if (!match.Success)
        {
            lat = double.NaN;
            lon = double.NaN;
            Console.WriteLine("[Bolt] Coordinates not found");
        }
        else
        {
            lat = double.Parse(match.Groups["lat"].Value);
            lon = double.Parse(match.Groups["lon"].Value);
        }

        //nyitvatartás
        Dictionary<string,Nyitvatartas> nyitvatartasok = [];
        //+1 dl valahol elöl
        for (int i = 1; i < timeNodes.Count; i += 2)
        {
            var day = timeNodes[i].InnerText.Trim();
            var hours = timeNodes[i + 1].InnerText.Trim().Split('-');

            nyitvatartasok.Add(day,new Nyitvatartas(TimeOnly.Parse(hours[0]), TimeOnly.Parse(hours[1])));
        }

        Console.WriteLine($"[Bolt] Finished : {url}");
        return new Bolt
        {
            Location = locality,
            Street = street,
            Nyitvatartasok = nyitvatartasok,
            Latitude = lat,
            Longitude = lon,
        };
    }

    [GeneratedRegex(@"L\.marker\(\s*\[(?<lat>[\d\.]+),\s*(?<lon>[\d\.]+)\]")]
    private static partial Regex JsCoordArrRegex();
}