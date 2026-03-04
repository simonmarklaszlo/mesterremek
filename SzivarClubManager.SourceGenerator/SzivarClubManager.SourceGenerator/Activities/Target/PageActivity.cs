using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Targets;

namespace SzivarClubManager.SourceGenerator.Activities.Target;

public class PageActivity : Activity
{
    public INamedTypeSymbol ModelSymbol { get; }
    public string ModelName => ModelSymbol.Name;

    public PageActivity(INamedTypeSymbol pageActivitySymbol, INamedTypeSymbol modelSymbol) : base(pageActivitySymbol)
    {
        ModelSymbol = modelSymbol;
    }

    public string InstanceCreation(string factoryProviderVariableName) => $"new {Name}({factoryProviderVariableName}.GetPageFactory<{ModelName}>())";

    public new static IncrementalValuesProvider<PageActivity> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static PageActivity? IsTarget(GeneratorSyntaxContext context)
    {
        const string pageActivityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.PageActivityCollectionItemAttribute";

        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var activityAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(pageActivityCollectionItemAttribute);
        if (activityAttributeSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, activityAttributeSymbol)) continue;

            if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol })
            {
                return new PageActivity(typeSymbol, modelTypeSymbol);
            }
        }

        return null;
    }
}