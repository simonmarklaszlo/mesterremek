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
    public static CustomPgPoint FromPgPoint(Point point) => new(point.X, point.Y);
    public override string ToString() => $"{Math.Round(Lon, 2)} {Math.Round(Lat, 2)}";
}