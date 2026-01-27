using System;
using SzivarClubManager.Datasources.Database.Factories;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Models;

[PageActivity]
public record User(int Id, string Name, string Email, DateTime CreatedAt, Role Role);