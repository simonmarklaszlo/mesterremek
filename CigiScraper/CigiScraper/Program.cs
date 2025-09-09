using CigiScraper;

await LocalCache.Clean();

var scraper = new Scraper();
var res = await scraper.ScrapeFull();
var distinct = res.Distinct().ToArray();

var completeData = distinct.Where(x => !x.HasError).ToArray();
var incompleteData = distinct.Where(x => x.HasError).ToArray();
var fatalData = distinct.Where(x => x.FatalError).ToArray();


var archiver = new Archiver("archive");
await archiver.ArchiveToCsv("full.csv",completeData);
await archiver.ArchiveToCsv("incomplete.csv",incompleteData);
await archiver.ArchiveToCsv("fatal.csv",fatalData);


Console.WriteLine("===============");
Console.WriteLine($"{"Total Data Count",-30}{res.Length}");
Console.WriteLine($"{"Total Distinct Data Count",-30}{distinct.Length}");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"{"Total Complete Data Count",-30}{completeData.Length}");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"{"Total Incomplete Data Count",-30}{incompleteData.Length}");
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"{"Total Fatal Data Count",-30}{fatalData.Length}");
Console.WriteLine("===============");
