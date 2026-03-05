using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.CommonTargets;

namespace SzivarClubManager.SourceGenerator.Generators.Activities.Target;

public class PageActivity : Activity
{
    private readonly INamedTypeSymbol _modelSymbol;

    private PageActivity(INamedTypeSymbol pageActivitySymbol, INamedTypeSymbol modelSymbol, string displayName, byte? orderGroup) : base(pageActivitySymbol, displayName, orderGroup)
    {
        _modelSymbol = modelSymbol;
    }

    public string InstanceCreation(string factoryProviderVariableName) => $"new {ActivitySymbol.GlobalName()}({factoryProviderVariableName}.GetPageFactory<{_modelSymbol.GlobalName()}>())";

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

            if (attribute.ConstructorArguments.Length >= 2 &&
                attribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol } &&
                attribute.ConstructorArguments[1].Value is string displayName)
            {
                if (attribute.ConstructorArguments.Length == 3 && attribute.ConstructorArguments[2].Value is byte orderGroup)
                {
                    return new PageActivity(typeSymbol, modelTypeSymbol, displayName, orderGroup);
                }

                return new PageActivity(typeSymbol, modelTypeSymbol, displayName, null);
            }
        }

        return null;
    }
}