namespace CigiScraper.Model;

public class Bolt
{
    public string Url { get; }
    public double Longitude { get; }
    public double Latitude { get; }
    public string Location { get; }
    public string Street { get; }
    public Nyitvatartas[] Nyitvatartasok { get; }

    public BoltError Error { get; }
    public bool HasError => Error is not BoltError.None;
    public bool FatalError => Error.HasFlag(BoltError.Longitude) &&
                              Error.HasFlag(BoltError.Latitude) &&
                              Error.HasFlag(BoltError.Location) &&
                              Error.HasFlag(BoltError.Street) &&
                              Error.HasFlag(BoltError.Nyitvatartasok);

    public Bolt(string url, double longitude, double latitude, string location, string street, Nyitvatartas[] nyitvatartasok)
    {
        Url = url;
        Longitude = longitude;
        Latitude = latitude;
        Location = location;
        Street = street;
        Nyitvatartasok = nyitvatartasok;

        Error = CheckFullError(longitude, latitude, location, street, nyitvatartasok);
    }


    public string ToCsvLine()
    {
        return $"{Longitude};{Latitude};{Location};{Street};{Nyitvatartas.ToCsvLine(Nyitvatartasok)}";
    }

    public bool EqualsCoordinates(Bolt other)
    {
        const double tolerance = 0.1;
        return Math.Abs(Longitude - other.Longitude) < tolerance &&
               Math.Abs(Latitude - other.Latitude) < tolerance;
    }
    public override string ToString()
    {
        return $"{Location}, {Street}";
    }
    public override bool Equals(object? obj)
    {
        if (obj is not Bolt other) return false;
        return EqualsCoordinates(other) &&
               Location == other.Location &&
               Street == other.Street &&
               Nyitvatartasok.SequenceEqual(other.Nyitvatartasok);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Url, Longitude, Latitude, Location, Street, Nyitvatartasok, (int)Error);
    }

    private static BoltError CheckFullError(double longitude, double latitude, string location, string street, Nyitvatartas[] nyitvatartasok)
    {
        var error = BoltError.None;

        if (double.IsNaN(longitude))
            error |= BoltError.Longitude;

        if (double.IsNaN(latitude))
            error |= BoltError.Latitude;

        if (string.IsNullOrEmpty(location))
            error |= BoltError.Location;

        if (string.IsNullOrEmpty(street))
            error |= BoltError.Street;

        if (nyitvatartasok.Length == 0)
            error |= BoltError.Nyitvatartasok;

        return error;
    }
}

[Flags]
public enum BoltError
{
    None = 0,
    Longitude = 1 << 1,   // 2
    Latitude = 1 << 2,    // 4
    Location = 1 << 3,    // 8
    Street = 1 << 4,      // 16
    Nyitvatartasok = 1 << 5 // 32
}