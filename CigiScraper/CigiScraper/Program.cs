using CigiScraper;

var res = await new Scraper("https://nemzetidohanyboltkereso.hu/trafik-lista").Run();



Console.WriteLine(res.Length);