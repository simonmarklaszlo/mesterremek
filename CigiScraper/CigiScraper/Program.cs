using CigiScraper;
using CigiScraper.Db;
using CigiScraper.LocalData;
using CigiScraper.Model.Place;
using CigiScraper.Model.Shop;
using Scraper = CigiScraper.Scraping.Scraper;

//cylex.hu
//nyitva.hu

var comp = await GetMixed();

//await DbUploader.Upload(comp);



return;

async Task<UnofficialShop[]> Scrape()
{
    var scraper = new Scraper();
    var res = await scraper.ScrapeAny();
    res = res.EliminateDuplicates();


    var completeData = res.Where(x => !x.HasError).ToArray();
    var incompleteData = res.Where(x => x.HasError).ToArray();
    var fatalData = res.Where(x => x.FatalError).ToArray();


    await Logger.Flush();

    var archiver = new Archiver("archive/scraped");
    await archiver.ArchiveToCsv("full.csv", completeData);
    await archiver.ArchiveToCsv("incomplete.csv", incompleteData);
    await archiver.ArchiveToCsv("fatal.csv", fatalData);
    await archiver.ArchiveCoordsToCsv("full_coords.csv", completeData);


    return completeData;
}

async Task<UnofficialShop[]> GetUnofficialFull()
{
    var archiver = new Archiver("archive/scraped");
    var res = await archiver.ReadBackFromArchive("full.csv");
    if (res.Length == 0)
    {
        res = await Scrape();
    }

    Console.WriteLine("Loaded Unofficial");

    return res;
}

async Task<OfficialShop[]> GetOfficialFull()
{
    var postal = PostalLocation.ParseFile("archive/input/postalcodes.csv");
    var officialShops = OfficialShop.ParseFile("archive/input/official_bolt.csv");

    await Task.WhenAll(postal, officialShops);

    var postalData = postal.Result;
    var officialData = officialShops.Result;

    officialData.MapTo(postalData);

    Console.WriteLine("Loaded Official");

    return officialData.Where(x => x.PostalLocation is not null).ToArray();
}

async Task<MixedShop[]> GetMixed()
{
    var ot = GetOfficialFull();
    var ut = GetUnofficialFull();
    await Task.WhenAll(ot, ut);

    var official = ot.Result;
    var unofficial = ut.Result;

    var exact = official.FindExactMatches(unofficial).OrderBy(x => x.City).ThenBy(x => x.Address).ToArray();
    var mix = official.MixWith(unofficial).OrderBy(x => x.City).ThenBy(x => x.Address).ToArray();
    var complete = ((MixedShop[])[..exact,..mix]).OrderBy(x => x.City).ThenBy(x => x.Address).ToArray();
    var old = official.MixOld(unofficial).OrderBy(x => x.City).ThenBy(x => x.Address).ToArray();

    var archiver = new Archiver("archive");
    archiver.ClearFiles();

    await archiver.ArchiveToCsv("exact.csv", exact);
    await archiver.ArchiveToCsv("mix.csv", mix);
    await archiver.ArchiveToCsv("complete.csv", complete);
    await archiver.ArchiveToCsv("comp_old.csv", old);


    return [];
}