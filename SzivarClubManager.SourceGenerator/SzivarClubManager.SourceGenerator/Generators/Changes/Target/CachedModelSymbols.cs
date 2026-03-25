using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.Generators.Changes.Target;

public class CachedModelSymbols
{
    public INamedTypeSymbol ModelAttributeSymbol { get; }

    public CachedModelSymbols(INamedTypeSymbol modelAttributeSymbol)
    {
        ModelAttributeSymbol = modelAttributeSymbol;
    }


    public static (CachedModelSymbols? cachedSymbols, List<string> missingSymbols) Load(Compilation compilation)
    {
        var modelAttributeSymbol = compilation.GetTypeByMetadataName(StringReferences.ModelAttribute);


        if (modelAttributeSymbol is null)
        {
            List<string> missing = [];

            if (modelAttributeSymbol is null) missing.Add(StringReferences.ModelAttribute);

            return (null, missing);
        }

        return (new CachedModelSymbols(modelAttributeSymbol), []);
    }
}