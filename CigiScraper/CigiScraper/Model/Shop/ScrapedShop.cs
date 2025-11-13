using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public record ScrapedShop(string Url, string? Name, string City, string Address, OpeningSchedule[] Schedules, double Longitude, double Latitude) : Shop(Name, City, Address, Schedules, Longitude, Latitude);