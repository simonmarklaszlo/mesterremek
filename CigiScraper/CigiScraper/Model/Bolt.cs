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
    public string GetErrorString()
    {
        if (Error is BoltError.None) return "None";
        var errorString = "";
        if ((Error & BoltError.Longitude) != 0) errorString += "Longitude, ";
        if ((Error & BoltError.Latitude) != 0) errorString += "Latitude, ";
        if ((Error & BoltError.Location) != 0) errorString += "Location, ";
        if ((Error & BoltError.Street) != 0) errorString += "Street, ";
        if ((Error & BoltError.Nyitvatartasok) != 0) errorString += "Nyitvatartasok, ";
        return errorString.TrimEnd(',', ' ');
    }
    public override string ToString()
    {
        return $"{Location}, {Street}";
    }

    public static BoltError CheckFullError(double longitude, double latitude, string location, string street, Nyitvatartas[] nyitvatartasok)
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

    public bool Equals(Bolt? other)
    {
        if (other is null) return false;

        return Math.Abs(Longitude - other.Longitude) < 0.1 &&
               Math.Abs(Latitude - other.Latitude) < 0.1 &&
               Location == other.Location &&
               Street == other.Street &&
               Nyitvatartasok.SequenceEqual(other.Nyitvatartasok);
    }

    public override bool Equals(object? obj) => Equals(obj as Bolt);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Longitude.GetHashCode();
            hash = hash * 23 + Latitude.GetHashCode();
            hash = hash * 23 + Location.GetHashCode();
            hash = hash * 23 + Street.GetHashCode();

            if (Nyitvatartasok != null)
            {
                foreach (var n in Nyitvatartasok)
                    hash = hash * 23 + n.GetHashCode();
            }

            return hash;
        }
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