using System;

namespace SzivarClubManager.SourceGenerator.Generators.Factories.Target;

[Flags]
public enum FactoryType
{
    Unspecified = 0,
    Page = 1 << 0,
    Helper = 1 << 1,
}

public static class FactoryTypeExtensions
{
    public static string ToTypeName(this FactoryType type) => type switch
    {
        FactoryType.Page => "PageFactory",
        FactoryType.Helper => "HelperFactory",
        FactoryType.Page | FactoryType.Helper => "Page-HelperFactory",
        FactoryType.Unspecified => "Unspecified",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}