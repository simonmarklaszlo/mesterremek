using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public record Shop(string? Name, string City, string Address, OpeningSchedule[] Schedules, double Longitude, double Latitude)
{
    public string ToCsvLine() => $"{Name ?? "NULL"};{City};{Address};{Schedules.Length};{Longitude};{Latitude};{OpeningSchedule.ToCsvLine(Schedules)}";
}