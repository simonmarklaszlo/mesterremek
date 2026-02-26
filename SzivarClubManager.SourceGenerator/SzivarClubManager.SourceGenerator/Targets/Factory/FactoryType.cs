using System;

namespace SzivarClubManager.SourceGenerator.Targets.Factory;

public enum FactoryType
{
    Unknown,
    Page,
    Helper
}

public static class FactoryTypeExtensions
{
    public static string ToTypeName(this FactoryType type) => type switch
    {
        FactoryType.Unknown => "Unknown",
        FactoryType.Page => "PageFactory",
        FactoryType.Helper => "HelperFactory",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}