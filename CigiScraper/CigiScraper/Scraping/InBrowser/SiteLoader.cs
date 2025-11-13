using System.Xml.XPath;
using CigiScraper.LocalData;
using Microsoft.Playwright;

namespace CigiScraper.Scraping.InBrowser;

public abstract class SiteLoader
{
    protected const string ArchiveBase = "./archive/userdata";
    protected readonly HtmlCache HtmlCache;
    protected readonly Logger Logger;
    private readonly IBrowserContext _context;
    protected readonly IPage Page;

    protected SiteLoader(string name, IBrowserContext context, IPage page)
    {
        HtmlCache = new HtmlCache(name);
        Logger = new Logger(Path.Combine("loader",name));
        _context = context;
        Page = page;
    }

    public abstract Task Load();

    protected async Task DownloadCurrentPage()
    {
        var html = await Page.ContentAsync();
        await HtmlCache.SavePage(Page.Url, html);
    }

    protected static void ValidateXpath(string xpath)
    {
        try
        {
            XPathExpression.Compile(xpath);
        }
        catch (XPathException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static async Task RunAll()
    {
        var playwright = await Playwright.CreateAsync();

        var cylex = await CylexLoader.CreateNew(playwright);

        await cylex.Load();
    }
}