using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Targets.Factory;

public class Factory
{
    public string Name => FactorySymbol.Name;
    public INamedTypeSymbol FactorySymbol { get; }
    public INamedTypeSymbol ModelSymbol { get; }
    public FactoryType FactoryType { get; }
    public INamedTypeSymbol[] Dependencies { get; }

    public Factory(INamedTypeSymbol factorySymbol, INamedTypeSymbol modelSymbol, FactoryType factoryType, INamedTypeSymbol[] dependencies)
    {
        FactorySymbol = factorySymbol;
        ModelSymbol = modelSymbol;
        FactoryType = factoryType;
        Dependencies = dependencies;
    }

    public string VariableDeclaration(string dbConnVarName) => $"{FactorySymbol.GlobalName()} {FactorySymbol.PascalCaseName()} = new {FactorySymbol.GlobalName()}({dbConnVarName});";

    public string VariableDeclaration(string dbConnVarName, IEnumerable<string> parameters) =>
        $"{FactorySymbol.GlobalName()} {FactorySymbol.PascalCaseName()} = new {FactorySymbol.GlobalName()}({dbConnVarName}, {string.Join(", ", parameters)});";

    public static IncrementalValuesProvider<Factory> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static Factory? IsTarget(GeneratorSyntaxContext context)
    {
        const string factoryOfAttribute = "SzivarClubManager.SourceGeneration.FactoryOfAttribute";
        const string factoryInterface = "SzivarClubManager.Datasources.Factory.IFactory";
        const string pageFactoryInterface = "SzivarClubManager.Datasources.Factory.IPageFactory`1";
        const string helperFactoryInterface = "SzivarClubManager.Datasources.Factory.IHelperFactory`1";

        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;

        var factoryAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(factoryOfAttribute);
        var factoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(factoryInterface);
        var pageFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(pageFactoryInterface);
        var helperFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(helperFactoryInterface);
        if (factoryAttributeSymbol is null || factoryInterfaceSymbol is null || pageFactoryInterfaceSymbol is null || helperFactoryInterfaceSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, factoryAttributeSymbol)) continue;


            if (
                attribute.ConstructorArguments.Length >= 1 &&
                attribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol }
            )
            {
                INamedTypeSymbol[] dependencies = [];

                if (attribute.ConstructorArguments.Length >= 2)
                {
                    var depsArg = attribute.ConstructorArguments[1];

                    if (depsArg is { Kind: TypedConstantKind.Array, IsNull: false })
                    {
                        dependencies = depsArg.Values
                            .Select(v => v.Value)
                            .OfType<INamedTypeSymbol>()
                            .ToArray();
                    }
                }

                FactoryType factoryType = FactoryType.Unspecified;

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, pageFactoryInterfaceSymbol)))
                {
                    factoryType |= FactoryType.Page;
                }

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, helperFactoryInterfaceSymbol)))
                {
                    factoryType |= FactoryType.Helper;
                }

                return new Factory(typeSymbol, modelTypeSymbol, factoryType, dependencies);
            }
        }

        return null;
    }
}