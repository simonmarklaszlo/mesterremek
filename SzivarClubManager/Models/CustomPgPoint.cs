using System;
using NetTopologySuite.Geometries;

namespace SzivarClubManager.Models;

public record CustomPgPoint(double X, double Y)
{
    private const int Srid = 4326;
    public Point ToPgPoint()
    {
        return new Point(X, Y)
        {
            SRID = Srid
        };
    }

    public override string ToString()
    {
        return $"{Math.Round(X, 2)} {Math.Round(Y, 2)}";
    }

    public static CustomPgPoint FromPgPoint(Point point)
    {
        return new CustomPgPoint(point.X, point.Y);
    }
}