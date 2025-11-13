using CigiScraper.Model.Time;

namespace CigiScraper.Model.Shop;

public class UnofficialShop3
{
    public string? Url { get; }
    public double Longitude { get; }
    public double Latitude { get; }
    public string City { get; }
    public string Address { get; }
    public OpeningSchedule[] OpeningSchedules { get; }

    public ShopError Error { get; }
    public bool HasError => Error is not ShopError.None;

    public bool FatalError => Error.HasFlag(ShopError.Longitude) &&
                              Error.HasFlag(ShopError.Latitude) &&
                              Error.HasFlag(ShopError.Location) &&
                              Error.HasFlag(ShopError.Street) &&
                              Error.HasFlag(ShopError.OpeningSchedules);


    public UnofficialShop3(string url, double longitude, double latitude, string city, string address,
        OpeningSchedule[] openingSchedules)
    {
        Url = url;
        Longitude = longitude;
        Latitude = latitude;
        City = city;
        Address = address;
        OpeningSchedules = openingSchedules;

        Error = CheckFullError(longitude, latitude, city, address, openingSchedules);
    }


    public override string ToString() => $"{Latitude} {Longitude}";

    public string ToCsvLine() =>
        $"{Longitude};{Latitude};{City};{Address};{OpeningSchedule.ToCsvLine(OpeningSchedules)}";

    public override bool Equals(object? obj)
    {
        if (obj is not UnofficialShop3 other) return false;
        return EqualsCoordinates(other) && City == other.City && Address == other.Address &&
               OpeningSchedules.SequenceEqual(other.OpeningSchedules);
    }

    public bool EqualsCoordinates(UnofficialShop3 other)
    {
        const double tolerance = 0.001;
        return Math.Abs(Longitude - other.Longitude) < tolerance &&
               Math.Abs(Latitude - other.Latitude) < tolerance;
    }

    private static ShopError CheckFullError(double longitude, double latitude, string location, string street,
        OpeningSchedule[] openingSchedules)
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

    [Flags]
    public enum ShopError
    {
        None = 0,
        Longitude = 1 << 1, // 2
        Latitude = 1 << 2, // 4
        Location = 1 << 3, // 8
        Street = 1 << 4, // 16
        OpeningSchedules = 1 << 5 // 32
    }
}