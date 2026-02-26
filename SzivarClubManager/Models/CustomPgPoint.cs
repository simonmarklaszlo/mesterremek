using System;
using System.Globalization;
using NetTopologySuite.Geometries;

namespace SzivarClubManager.Models;

public record CustomPgPoint(double Lon, double Lat)
{
    private const int Srid = 4326;

    public string LongitudeString => field ??= Lon.ToString(CultureInfo.CurrentCulture);
    public string LatitudeString => field ??= Lat.ToString(CultureInfo.CurrentCulture);


    public Point ToPgPoint() => new(Lon, Lat) { SRID = Srid };
    public CustomPgPoint Copy() => new(Lon, Lat);
    private const double Tolerance = 0.00001;
    // public bool EqualsPoints(double lon, double lat) => Math.Abs(Lon - lon) < Tolerance && Math.Abs(Lat - lat) < Tolerance;
    public bool IsApproximatelyEqual(double lon, double lat)
    {
        const int maxDistanceMeters = 10;
        const double r = 6371000; // Earth radius in meters
        double dLat = (lat - Lat) * Math.PI / 180;
        double dLon = (lon - Lon) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Lat * Math.PI / 180) * Math.Cos(lat * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = r * c;
        return distance <= maxDistanceMeters;
    }

    public static CustomPgPoint FromPgPoint(Point point) => new(point.X, point.Y);
    public override string ToString() => $"{Math.Round(Lon, 2)} {Math.Round(Lat, 2)}";
}