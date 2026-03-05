using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator;

public static class TypeSymbolExtensions
{
    extension(INamedTypeSymbol symbol)
    {
        public string GlobalName() => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        public string PascalCaseName() => char.ToLower(symbol.Name[0]) + symbol.Name.Substring(1);
    }
}