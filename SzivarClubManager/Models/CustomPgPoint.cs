using System;
using NetTopologySuite.Geometries;

namespace SzivarClubManager.Models;

public record CustomPgPoint(double X, double Y)
{
    private const int Srid = 4326;
    public Point ToPgPoint() => new(X, Y) { SRID = Srid };
    public CustomPgPoint Copy() => new(X, Y);
    public static CustomPgPoint FromPgPoint(Point point) => new(point.X, point.Y);
    public override string ToString() => $"{Math.Round(X, 2)} {Math.Round(Y, 2)}";
}