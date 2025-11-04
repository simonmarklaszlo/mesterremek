using CigiScraper.Model.Place;
using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public record UnofficialShop2(string Url, string? Name, ShopLocation Location, double Longitude, double Latitude, OpeningSchedule[] Schedule);