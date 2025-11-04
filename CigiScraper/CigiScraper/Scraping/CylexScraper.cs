using System.Globalization;
using System.Text.RegularExpressions;
using CigiScraper.Model.Place;
using CigiScraper.Model.Shop;
using CigiScraper.Model.Time;
using HtmlAgilityPack;

namespace CigiScraper.Scraping;

public partial class CylexScraper() : Scraper("Cylex")
{
    private const string PlacesBaseUrl = "https://www.cylex.hu/trafik/helyek/";

    protected override async Task<UnofficialShop[]> ScrapeFromOnline()
    {
        HashSet<string> shopUrlHashes = [];
        List<UnofficialShop2> shops = [];
        var placeUrls = await GetPlaceUrls();
        foreach (var placeUrl in placeUrls)
        {
            var shopUrls = await GetShopUrlsFromPlace(placeUrl);
            foreach (var shopUrl in shopUrls)
            {
                if (!shopUrlHashes.Add(shopUrl)) continue;
                shops.Add(await GetShopFromUrl(shopUrl));
            }
        }

        return [];
    }

    protected override Task<UnofficialShop[]> ScrapeFromCache()
    {
        return ScrapeFromOnline();
    }

    private async Task<IEnumerable<string>> GetPlaceUrls()
    {
        char[] letters = [..Enumerable.Range('A', 26).Select(x => (char)x), 'Á', 'É', 'Í', 'Ö', 'Ő', 'Ü', 'Ű'];

        IEnumerable<string> combined = [];

        foreach (var c in letters)
        {
            var urls = await GetPlaceUrlsForLetter(c);
            combined = combined.Concat(urls);
        }

        return combined;

        async Task<IEnumerable<string>> GetPlaceUrlsForLetter(char letter)
        {
            var htmlDocument = await LoadHtml(PlacesBaseUrl + letter);

            var urls = htmlDocument.DocumentNode
                .SelectNodes("//ul[contains(@class, 'row') and contains(@class, 'list-unstyled')]//a[@href]")
                ?.Select(a => a.GetAttributeValue("href", string.Empty))
                .Where(x => !string.IsNullOrEmpty(x));

            return urls ?? [];
        }
    }

    private async Task<IEnumerable<string>> GetShopUrlsFromPlace(string placeUrl)
    {
        var htmlDocument = await LoadHtml(placeUrl);

        var regex = RegexShopUrlInOnclick();

        var s = htmlDocument.DocumentNode
            .SelectNodes("//div[contains(@class, 'lm-comp') and contains(@class, 'position-relative') and contains(@class, 'basic') and @onclick]")
            ?.Select(x => x.GetAttributeValue("onclick", string.Empty))
            .Select(x =>
            {
                var match = regex.Match(x);
                if (match.Success) return match.Groups[1].Value;
                return string.Empty;
            })
            .Where(x => !string.IsNullOrEmpty(x));

        return s ?? [];
    }

    private async Task<UnofficialShop2> GetShopFromUrl(string shopUrl)
    {
        var htmlDocument = await LoadHtml(shopUrl);

        var (name, city) = await ExtractShopNameAndCity(shopUrl, htmlDocument);
        var location = await ExtractShopLocation(shopUrl, htmlDocument);
        var (longitude, latitude) = await ExtractCoordinates(shopUrl, htmlDocument);
        var openingSchedule = await ExtractOpeningSchedule(shopUrl, htmlDocument);

        if (location.City == string.Empty && !string.IsNullOrWhiteSpace(city))
        {
            location = location with { City = city };
        }

        return new UnofficialShop2(shopUrl, name, location, longitude, latitude, openingSchedule);
    }

    private async Task<(string?, string?)> ExtractShopNameAndCity(string shopUrl, HtmlDocument htmlDocument)
    {
        var nameWithPlace = htmlDocument.DocumentNode
            .SelectNodes("//h1[@id='address']")
            .FirstOrDefault()
            ?.InnerText;

        string? name = null;
        string? city = null;
        if (nameWithPlace is not null || !string.IsNullOrEmpty(nameWithPlace))
        {
            var split = nameWithPlace.Split('-');
            if (split.Length == 2)
            {
                name = split[0];
                city = split[^1];
            }
            else
            {
                name = nameWithPlace;
            }
        }
        else
        {
            await Logger.Error($"No name or city found for {shopUrl}");
        }

        return (name, city);
    }

    private async Task<ShopLocation> ExtractShopLocation(string shopUrl, HtmlDocument htmlDocument)
    {
        var div = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='cp-street']");

        string address = string.Empty;
        string city = string.Empty;
        int postalCodeInt = -1;
        if (div is not null)
        {
            var spans = div.SelectNodes(".//span");

            if (spans.Count == 3)
            {
                address = spans[0].InnerText.Trim();
                string postalCode = spans[1].InnerText.Trim();
                if (int.TryParse(postalCode, out int p))
                {
                    postalCodeInt = p;
                }
                else
                {
                    await Logger.Warn($"Unable to parse postal code ({postalCode}) for {shopUrl}");
                }

                city = spans[2].InnerText.Trim();
            }
            else
            {
                if (spans.Count == 0)
                {
                    await Logger.Error($"Address/PostalCode/City not found. No spans found for {shopUrl}");
                }
                else
                {
                    await Logger.Warn($"Address/PostalCode/City missing. Only {spans.Count} spans found for {shopUrl}");
                    if (spans.Count == 2)
                    {
                        address = spans[0].InnerText.Trim();
                        string postalCode = spans[1].InnerText.Trim();

                        if (int.TryParse(postalCode, out int p))
                        {
                            postalCodeInt = p;
                        }
                        else
                        {
                            await Logger.Warn($"Unable to parse postal code ({postalCode}) for {shopUrl}");
                            city = postalCode;
                            postalCode = string.Empty;
                        }
                    }
                    else
                    {
                        address = spans[0].InnerText.Trim();
                    }
                }
            }
        }
        else
        {
            await Logger.Error($"Address/PostalCode/City not found. No div#cp-street found for {shopUrl}");
        }

        return new ShopLocation(postalCodeInt, address, city);
    }

    private async Task<OpeningSchedule[]> ExtractOpeningSchedule(string shopUrl, HtmlDocument htmlDocument)
    {
        var table = htmlDocument.DocumentNode
            .SelectSingleNode("//table[contains(@class, 'opening-hours')]//tbody");

        if (table is null)
        {
            await Logger.Error($"No table/tbody found for {shopUrl}");
            return [];
        }

        var rows = table.SelectNodes(".//tr");
        if (rows.Count == 0)
        {
            await Logger.Error($"No rows found in {table.OuterHtml} for {shopUrl}");
            return [];
        }


        List<OpeningSchedule> schedules = new(7);

        foreach (var node in rows)
        {
            var day = node.SelectSingleNode(".//td//span").InnerText.Trim();
            var timeStr = node.SelectSingleNode(".//td//div[contains(@class, 'interval-field')]").InnerText.Trim();

            schedules.Add(new OpeningSchedule(day, OpeningHours.Parse(timeStr, shopUrl, schedules)));
        }

        return schedules.ToArray();
    }

    private async Task<(double, double)> ExtractCoordinates(string shopUrl, HtmlDocument htmlDocument)
    {
        HtmlNode? node = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='osm-map']");
        if (node is null)
        {
            await Logger.Error($"No div#osm-map found for {shopUrl}");
            return (double.NaN, double.NaN);
        }

        string src = node.GetAttributeValue("data-map-src", string.Empty);
        if (string.IsNullOrEmpty(src))
        {
            await Logger.Error($"No data-map-src in {node.OuterHtml} found for {shopUrl}");
            return (double.NaN, double.NaN);
        }

        var match = RegexCoordinates().Match(src);
        if (match.Success)
        {
            double longitude = double.Parse(match.Groups["lon"].Value, CultureInfo.InvariantCulture);
            double latitude = double.Parse(match.Groups["lat"].Value, CultureInfo.InvariantCulture);

            return (longitude, latitude);
        }

        await Logger.Error($"No coordinates found in {src} for {shopUrl}");
        return (double.NaN, double.NaN);
    }


    [GeneratedRegex("location.href='([^']+)'", RegexOptions.Compiled)]
    private static partial Regex RegexShopUrlInOnclick();

    [GeneratedRegex(@"[?&]marker=(?<lon>-?\d+\.\d+),(?<lat>-?\d+\.\d+)")]
    private static partial Regex RegexCoordinates();
}