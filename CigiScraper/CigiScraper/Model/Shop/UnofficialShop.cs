using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public class UnofficialShop
{
    public string? Url { get; }
    public double Longitude { get; }
    public double Latitude { get; }
    public string LocationName { get; }
    public string Address { get; }
    public OpeningSchedule[] OpeningSchedules { get; }

    public ShopError Error { get; }
    public bool HasError => Error is not ShopError.None;
    public bool FatalError => Error.HasFlag(ShopError.Longitude) &&
                              Error.HasFlag(ShopError.Latitude) &&
                              Error.HasFlag(ShopError.Location) &&
                              Error.HasFlag(ShopError.Street) &&
                              Error.HasFlag(ShopError.OpeningSchedules);


    public UnofficialShop(string url, double longitude, double latitude, string locationName, string address, OpeningSchedule[] openingSchedules)
    {
        Url = url;
        Longitude = longitude;
        Latitude = latitude;
        LocationName = locationName;
        Address = address;
        OpeningSchedules = openingSchedules;

        Error = CheckFullError(longitude, latitude, locationName, address, openingSchedules);
    }


    public bool EqualsCoordinates(UnofficialShop other)
    {
        const double tolerance = 0.1;
        return Math.Abs(Longitude - other.Longitude) < tolerance &&
               Math.Abs(Latitude - other.Latitude) < tolerance;
    }
    public string ToCsvLine() => $"{Longitude};{Latitude};{LocationName};{Address};{OpeningSchedule.ToCsvLine(OpeningSchedules)}";
    public override string ToString() => $"{LocationName}, {Address}";
    public override bool Equals(object? obj)
    {
        if (obj is not UnofficialShop other) return false;
        return EqualsCoordinates(other) &&
               LocationName == other.LocationName &&
               Address == other.Address &&
               OpeningSchedules.SequenceEqual(other.OpeningSchedules);
    }
    public override int GetHashCode() => HashCode.Combine(Url, Longitude, Latitude, LocationName, Address, OpeningSchedules, (int)Error);


    private static ShopError CheckFullError(double longitude, double latitude, string location, string street, OpeningSchedule[] openingSchedules)
    {
        var error = ShopError.None;

        if (double.IsNaN(longitude))
            error |= ShopError.Longitude;

        if (double.IsNaN(latitude))
            error |= ShopError.Latitude;

        if (string.IsNullOrEmpty(location))
            error |= ShopError.Location;

        if (string.IsNullOrEmpty(street))
            error |= ShopError.Street;

        if (openingSchedules.Length == 0)
            error |= ShopError.OpeningSchedules;

        return error;
    }
}

[Flags]
public enum ShopError
{
    None = 0,
    Longitude = 1 << 1,   // 2
    Latitude = 1 << 2,    // 4
    Location = 1 << 3,    // 8
    Street = 1 << 4,      // 16
    OpeningSchedules = 1 << 5 // 32
}