using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Activities.Target;

public class PageActivity : Activity
{
    private const string PageActivityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.PageActivityCollectionItemAttribute";

    public string ModelType { get; }

    public PageActivity(string displayName, string viewModelType, string modelType) : base(displayName, viewModelType)
    {
        ModelType = modelType;
    }


    public string InstanceCreation(string factoryProviderVariableName) => $"new {ViewModelType}({factoryProviderVariableName}.GetPageFactory<{ModelType}>())";

    public new static PageActivity? IsTarget(GeneratorSyntaxContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var attributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(PageActivityCollectionItemAttribute);
        if (attributeSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol })
            {
                return new PageActivity($"{modelTypeSymbol.Name}s", typeSymbol.Name, modelTypeSymbol.Name);
            }
        }

        return null;
    }
}