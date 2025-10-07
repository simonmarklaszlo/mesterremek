using CigiScraper;
using CigiScraper.Model;
using CigiScraper.Model.Place;
using CigiScraper.Model.Shop;

var comp = await CreateComplete();

return;

async Task ScrapeDefault(bool eraseCacheAfter)
{
    await LocalCache.Clean();

    var scraper = new Scraper();
    var res = await scraper.ScrapeFullHybrid();
    UnofficialShop?[] resNullable = res.Select(UnofficialShop? (x) => x).ToArray();
    for (var i = 0; i < resNullable.Length; i++)
    {
        for (var j = 0; j < resNullable.Length; j++)
        {
            if (i == j) continue;
            bool equal = resNullable[i]?.Equals(resNullable[j]) ?? false;
            if (equal) resNullable[i] = null;
        }
    }

    UnofficialShop[] distinct = resNullable.OfType<UnofficialShop>().ToArray();


    var completeData = distinct.Where(x => !x.HasError).ToArray();
    var incompleteData = distinct.Where(x => x.HasError).ToArray();
    var fatalData = distinct.Where(x => x.FatalError).ToArray();


    var archiver = new Archiver("archive/scraped");
    archiver.DeleteAllArchives();
    await archiver.ArchiveToCsv("full.csv", completeData);
    await archiver.ArchiveCoordsToCsv("full_coords.csv", completeData);
    await archiver.ArchiveToCsv("incomplete.csv", incompleteData);
    await archiver.ArchiveToCsv("fatal.csv", fatalData);

    await ReadFromArchive();

    if (eraseCacheAfter) LocalCache.Detete();
}

async Task ScrapeLocal()
{
    var scraper = new Scraper();
    var res = await scraper.ScrapeFromCache();
    UnofficialShop?[] resNullable = res.Select(UnofficialShop? (x) => x).ToArray();
    for (var i = 0; i < resNullable.Length; i++)
    {
        for (var j = 0; j < resNullable.Length; j++)
        {
            if (i == j) continue;
            bool equal = resNullable[i]?.Equals(resNullable[j]) ?? false;
            if (equal) resNullable[i] = null;
        }
    }

    UnofficialShop[] distinct = resNullable.OfType<UnofficialShop>().ToArray();


    var completeData = distinct.Where(x => !x.HasError).ToArray();
    var incompleteData = distinct.Where(x => x.HasError).ToArray();
    var fatalData = distinct.Where(x => x.FatalError).ToArray();


    var archiver = new Archiver("archive/scraped");
    archiver.DeleteAllArchives();
    await archiver.ArchiveToCsv("full.csv", completeData);
    await archiver.ArchiveCoordsToCsv("full_coords.csv", completeData);
    await archiver.ArchiveToCsv("incomplete.csv", incompleteData);
    await archiver.ArchiveToCsv("fatal.csv", fatalData);

    await ReadFromArchive();
}

async Task ReadFromArchive()
{
    var archiver = new Archiver("archive/scraped");
    var full = await archiver.ReadBackFromArchive("full.csv");
    var incomplete = await archiver.ReadBackFromArchive("incomplete.csv");
    var fatal = await archiver.ReadBackFromArchive("fatal.csv");

    Console.WriteLine("===============");
    Console.WriteLine($"{"Total Data Count",-30}{full.Length + incomplete.Length + fatal.Length}");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{"Total Complete Data Count",-30}{full.Length}");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{"Total Incomplete Data Count",-30}{incomplete.Length}");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{"Total Fatal Data Count",-30}{fatal.Length}");
    Console.ResetColor();
    Console.WriteLine("===============");
}

async Task<UnofficialShop[]> GetUnofficialFull()
{
    var archiver = new Archiver("archive/scraped");
    var res = await archiver.ReadBackFromArchive("full.csv");
    if (res.Length == 0)
    {
        await ScrapeLocal();
        res = await archiver.ReadBackFromArchive("full.csv");
    }

    return res;
}

async Task<OfficialShop[]> GetOfficialMapped()
{
    var postal = PostalLocation.ParseFile("archive/input/postalcodes.csv");
    var officialShops = OfficialShop.ParseFile("archive/input/official_bolt.csv");

    await Task.WhenAll(postal, officialShops);

    var postalData = postal.Result;
    var officialData = officialShops.Result;

    officialData.MapTo(postalData);

    //var unmapped = officialData.Where(x => x.PostalLocation is null).ToArray();

    return officialData.Where(x => x.PostalLocation is not null).ToArray();
}

async Task<MixedShop[]> CreateComplete()
{
    var ot = GetOfficialMapped();
    var ut = GetUnofficialFull();
    await Task.WhenAll(ot, ut);

    var official = ot.Result;
    var unofficial = ut.Result;

    var res = official.MixWith(unofficial);

    res = res
        .OrderBy(x => x.LocationName)
        .ThenBy(x => x.Address)
        .ToArray();

    var resFiltered = res.EliminateDuplicates();

    var archiver = new Archiver("archive");
    await archiver.ArchiveToCsv("completeWithDuplicated.csv", res);
    await archiver.ArchiveToCsv("complete.csv", resFiltered);

    return resFiltered;
}