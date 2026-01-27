using System;

namespace SzivarClubManager.Models;

public record Shop(
    int Id,
    string Name,
    string Address,
    string City,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    float Rating
);