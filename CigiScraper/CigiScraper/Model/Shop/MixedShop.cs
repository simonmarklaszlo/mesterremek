using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public class MixedShop
{
    public int PostalCode { get; }
    public string County { get; }
    public string City { get; }
    public string Address { get; }

    public string Name { get; }

    public double Latitude { get; }
    public double Longitude { get; }

    public OpeningSchedule[] Schedules { get; }

    public MixedShop(OfficialShop o, UnofficialShop u)
    {
        PostalCode = o.PostalLocation!.PostalCode;
        County = o.PostalLocation.County;
        City = o.PostalLocation.LocationName;
        Address = o.Location.Address;
        Name = o.Name;

        Latitude = u.Latitude;
        Longitude = u.Longitude;
        Schedules = u.OpeningSchedules;
    }

    public override string ToString()
    {
        return $"[{Name}] : {PostalCode}, {County}, {City}, {Address} - ({Latitude}, {Longitude})";
    }

    public string ToCsvLine() => $"{PostalCode};{County};{City};{Address};{Name};{Latitude};{Longitude};{OpeningSchedule.ToCsvLine(Schedules)}";

}