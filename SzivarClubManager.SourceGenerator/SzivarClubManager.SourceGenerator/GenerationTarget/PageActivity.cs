using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.GenerationTarget;

public class PageActivity : Activity
{
    private const string PageActivityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.PageActivityCollectionItemAttribute";
    private const string IdataSource = "SzivarClubManager.Datasources.IDataSource";


    public string ModelType { get; }
    private string PageExistsMethodName => $"{ModelType}PageExists";
    private string LastPageMethodName => $"GetLast{ModelType}Page";
    private string LoadPageMethodName => $"Get{ModelType}Page";


    public PageActivity(string displayName, string viewModelType, string modelType) : base(displayName, viewModelType)
    {
        ModelType = modelType;
    }


    public override string InstanceCreation(string dataSourceVariableName) =>
        $"new {ViewModelType}({dataSourceVariableName}, " +
        $"{dataSourceVariableName}.{PageExistsMethodName}, " +
        $"{dataSourceVariableName}.{LastPageMethodName}, " +
        $"{dataSourceVariableName}.{LoadPageMethodName})";

    public bool DataSourceHasMethods(Compilation compilation)
    {
        var dataSourceType = compilation.GetTypeByMetadataName(IdataSource);
        if (dataSourceType is null) return false;

        bool hasPageExists = dataSourceType
            .GetMembers(PageExistsMethodName)
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 2);

        bool hasLastPage = dataSourceType
            .GetMembers(LastPageMethodName)
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 1);

        bool hasLoadPage = dataSourceType
            .GetMembers(LoadPageMethodName)
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 2);

        return hasPageExists && hasLastPage && hasLoadPage;
    }


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