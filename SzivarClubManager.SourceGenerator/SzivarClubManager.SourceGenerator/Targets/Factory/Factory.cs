using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Targets.Factory;

public record Factory(
    INamedTypeSymbol FactorySymbol,
    INamedTypeSymbol ModelSymbol,
    bool DatabaseRequired,
    FactoryType FactoryType,
    INamedTypeSymbol[] Dependencies
)
{
    public string Name => FactorySymbol.Name;
    public string ModelName => ModelSymbol.Name;

    public string InstanceCreation(string databaseConnectionVariableName)
    {
        if (DatabaseRequired) return $"new {Name}({databaseConnectionVariableName})";
        return $"new {Name}()";
    }

    public string InstanceCreation(string databaseConnectionVariableName, IEnumerable<string> parameters)
    {
        if (DatabaseRequired) return $"new {Name}({databaseConnectionVariableName}, {string.Join(", ", parameters)})";
        return $"new {Name}({string.Join(", ", parameters)})";
    }


    public static IncrementalValuesProvider<Factory> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static Factory? IsTarget(GeneratorSyntaxContext context)
    {
        const string factoryOfAttribute = "SzivarClubManager.SourceGeneration.FactoryOfAttribute";
        const string pageFactoryInterface = "SzivarClubManager.Datasources.IPageFactory`1";
        const string helperFactoryInterface = "SzivarClubManager.Datasources.IHelperFactory`1";

        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var factoryAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(factoryOfAttribute);
        var pageFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(pageFactoryInterface);
        var helperFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(helperFactoryInterface);
        if (factoryAttributeSymbol is null || pageFactoryInterfaceSymbol is null || helperFactoryInterfaceSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, factoryAttributeSymbol)) continue;


            if (
                attribute.ConstructorArguments.Length >= 2 &&
                attribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol } &&
                attribute.ConstructorArguments[1].Value is bool databaseRequired
            )
            {
                INamedTypeSymbol[] dependencies = [];

                if (attribute.ConstructorArguments.Length >= 3)
                {
                    var depsArg = attribute.ConstructorArguments[2];

                    if (depsArg is { Kind: TypedConstantKind.Array, IsNull: false })
                    {
                        dependencies = depsArg.Values
                            .Select(v => v.Value)
                            .OfType<INamedTypeSymbol>()
                            .ToArray();
                    }
                }

                FactoryType factoryType = FactoryType.None;

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, pageFactoryInterfaceSymbol)))
                {
                    factoryType |= FactoryType.Page;
                }

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, helperFactoryInterfaceSymbol)))
                {
                    factoryType |= FactoryType.Helper;
                }

                if (factoryType != FactoryType.None)
                {
                    return new Factory(typeSymbol, modelTypeSymbol, databaseRequired, factoryType, dependencies);
                }
            }
        }

        return null;
    }
}