using System;
using NetTopologySuite.Geometries;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Models;

[PageActivity]
public record Shop(
    int Id,
    string Name,
    string Address,
    string City,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    CustomPgPoint Location
);