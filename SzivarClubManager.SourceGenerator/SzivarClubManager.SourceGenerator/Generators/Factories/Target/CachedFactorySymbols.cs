using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.Generators.Factories.Target;

public class CachedFactorySymbols
{
    public INamedTypeSymbol ModelInterfaceSymbol { get; }
    public INamedTypeSymbol FactoryAttributeSymbol { get; }
    public INamedTypeSymbol ModelFactoryInterfaceSymbol { get; }
    public INamedTypeSymbol HelperFactoryInterfaceSymbol { get; }
    public INamedTypeSymbol PageFactoryInterfaceSymbol { get; }
    public INamedTypeSymbol DatabaseConnectionSymbol { get; }

    public CachedFactorySymbols(INamedTypeSymbol modelInterfaceSymbol, INamedTypeSymbol factoryAttributeSymbol, INamedTypeSymbol modelFactoryInterfaceSymbol, INamedTypeSymbol helperFactoryInterfaceSymbol,
        INamedTypeSymbol pageFactoryInterfaceSymbol, INamedTypeSymbol databaseConnectionSymbol)
    {
        ModelInterfaceSymbol = modelInterfaceSymbol;
        FactoryAttributeSymbol = factoryAttributeSymbol;
        ModelFactoryInterfaceSymbol = modelFactoryInterfaceSymbol;
        HelperFactoryInterfaceSymbol = helperFactoryInterfaceSymbol;
        PageFactoryInterfaceSymbol = pageFactoryInterfaceSymbol;
        DatabaseConnectionSymbol = databaseConnectionSymbol;
    }


    public static (CachedFactorySymbols? cachedSymbols, List<string> missingSymbols) Load(Compilation compilation)
    {
        var modelInterfaceSymbol = compilation.GetTypeByMetadataName(StringReferences.ModelInterface);
        var factoryAttributeSymbol = compilation.GetTypeByMetadataName(StringReferences.FactoryAttribute);
        var factoryInterfaceSymbol = compilation.GetTypeByMetadataName(StringReferences.FactoryInterface);
        var modelFactoryInterfaceSymbol = compilation.GetTypeByMetadataName(StringReferences.ModelFactoryInterface);
        var helperFactoryInterfaceSymbol = compilation.GetTypeByMetadataName(StringReferences.HelperFactoryInterface);
        var pageFactoryInterfaceSymbol = compilation.GetTypeByMetadataName(StringReferences.PageFactoryInterface);
        var databaseConnectionSymbol = compilation.GetTypeByMetadataName(StringReferences.DatabaseConnection);


        if (
            modelInterfaceSymbol is null ||
            factoryAttributeSymbol is null ||
            factoryInterfaceSymbol is null ||
            modelFactoryInterfaceSymbol is null ||
            helperFactoryInterfaceSymbol is null ||
            pageFactoryInterfaceSymbol is null ||
            databaseConnectionSymbol is null
        )
        {
            List<string> missing = [];
            if (modelInterfaceSymbol is null) missing.Add(StringReferences.ModelInterface);
            if (factoryAttributeSymbol is null) missing.Add(StringReferences.FactoryAttribute);
            if (factoryInterfaceSymbol is null) missing.Add(StringReferences.FactoryInterface);
            if (modelFactoryInterfaceSymbol is null) missing.Add(StringReferences.ModelFactoryInterface);
            if (helperFactoryInterfaceSymbol is null) missing.Add(StringReferences.HelperFactoryInterface);
            if (pageFactoryInterfaceSymbol is null) missing.Add(StringReferences.PageFactoryInterface);
            if (databaseConnectionSymbol is null) missing.Add(StringReferences.DatabaseConnection);
            return (null, missing);
        }

        return (new CachedFactorySymbols(
            modelInterfaceSymbol,
            factoryAttributeSymbol,
            modelFactoryInterfaceSymbol,
            helperFactoryInterfaceSymbol,
            pageFactoryInterfaceSymbol,
            databaseConnectionSymbol
        ), []);
    }
}