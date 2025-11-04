using CigiScraper;
using CigiScraper.Db;
using CigiScraper.LocalData;
using CigiScraper.Model.Place;
using CigiScraper.Model.Shop;
using CigiScraper.Scraping;

//cylex.hu
//nyitva.hu

var comp = await GetMixed();

// await DbUploader.Upload(comp);



return;

async Task<UnofficialShop[]> Scrape()
{
    var scraper = new NdbScraper();
    var res = await scraper.ScrapeAny();
    res = res.EliminateDuplicates();


    var completeData = res.Where(x => !x.HasError).ToArray();
    var incompleteData = res.Where(x => x.HasError).ToArray();
    var fatalData = res.Where(x => x.FatalError).ToArray();


    await Logger2.Flush();

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

async Task<Shop[]> GetMixed()
{
    var ot = GetOfficialFull();
    var ut = GetUnofficialFull();
    await Task.WhenAll(ot, ut);

    var official = ot.Result;
    var unofficial = ut.Result;

    var withNames = unofficial.FindWithMatchingAddress(official);
    var unOffAsShop = unofficial
        .Select(x => new Shop(null,x.City,x.Address,x.OpeningSchedules,x.Longitude,x.Latitude))
        .ToArray();

    var complete = unOffAsShop
        .Concat(withNames)
        .GroupBy(x => x.Address)
        .Select(x => x.FirstOrDefault(y => y.Name is not null) ?? x.First())
        .ToArray();

    var archive = new Archiver("archive");
    archive.ClearFiles();
    await archive.ArchiveToCsv("complete.csv", complete);

    return complete;
}