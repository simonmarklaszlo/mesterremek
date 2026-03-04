using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Targets;

namespace SzivarClubManager.SourceGenerator.Activities.Target;

public class Activity
{
    public INamedTypeSymbol ActivitySymbol { get; }
    public string Name => ActivitySymbol.Name;
    public string DisplayName => Name.Replace("ActivityViewModel", "");

    protected Activity(INamedTypeSymbol activitySymbol)
    {
        ActivitySymbol = activitySymbol;
    }

    public string InstanceCreation() => $"new {Name}()";
    public static IncrementalValuesProvider<Activity> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static Activity? IsTarget(GeneratorSyntaxContext context)
    {
        const string activityCollectionItemAttribute = "SzivarClubManager.SourceGeneration.ActivityCollectionItemAttribute";

        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var activityAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(activityCollectionItemAttribute);
        if (activityAttributeSymbol is null) return null;

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, activityAttributeSymbol)) continue;

            if (attribute.ConstructorArguments.Length == 0)
            {
                return new Activity(typeSymbol);
            }
        }

        return null;
    }
}