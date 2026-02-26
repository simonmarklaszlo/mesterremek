using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Targets.Factory;

public record FakeFactory(
    INamedTypeSymbol FactorySymbol,
    INamedTypeSymbol ModelSymbol,
    FactoryType FactoryType,
    INamedTypeSymbol[] Dependencies
)
{
    public string Name => FactorySymbol.Name;
    public string ModelName => ModelSymbol.Name;

    public string InstanceCreation() => $"new {Name}()";
    public string InstanceCreation(IEnumerable<string> parameters) => $"new {Name}({string.Join(", ", parameters)})";


    public static IncrementalValuesProvider<FakeFactory> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static FakeFactory? IsTarget(GeneratorSyntaxContext context)
    {
        const string fakeFactoryOfAttribute = "SzivarClubManager.SourceGeneration.FakeFactoryOfAttribute";
        const string pageFactoryInterface = "SzivarClubManager.Datasources.IPageFactory`1";
        const string helperFactoryInterface = "SzivarClubManager.Datasources.IHelperFactory`1";

        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var fakeFactoryAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(fakeFactoryOfAttribute);
        var pageFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(pageFactoryInterface);
        var helperFactoryInterfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(helperFactoryInterface);
        if (fakeFactoryAttributeSymbol is null || pageFactoryInterfaceSymbol is null || helperFactoryInterfaceSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, fakeFactoryAttributeSymbol)) continue;


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

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, pageFactoryInterfaceSymbol)))
                {
                    return new FakeFactory(typeSymbol, modelTypeSymbol, FactoryType.Page, dependencies);
                }

                if (typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, helperFactoryInterfaceSymbol)))
                {
                    return new FakeFactory(typeSymbol, modelTypeSymbol, FactoryType.Helper, dependencies);
                }
            }
        }

        return null;
    }
}