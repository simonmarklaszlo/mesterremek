using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.CommonTargets;

namespace SzivarClubManager.SourceGenerator.Generators.Activities.Target;

public class Activity
{
    protected INamedTypeSymbol ActivitySymbol { get; }
    public string DisplayName { get; }
    public byte OrderGroup { get; }

    protected Activity(INamedTypeSymbol activitySymbol, string displayName, byte? orderGroup)
    {
        ActivitySymbol = activitySymbol;
        DisplayName = displayName;
        OrderGroup = orderGroup ?? byte.MaxValue;
    }

    public string InstanceCreation() => $"new {ActivitySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";

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

            if (attribute.ConstructorArguments.Length >= 1 && attribute.ConstructorArguments[0].Value is string displayName)
            {
                if (attribute.ConstructorArguments.Length == 2 && attribute.ConstructorArguments[1].Value is byte orderGroup)
                {
                    return new Activity(typeSymbol, displayName, orderGroup);
                }

                return new Activity(typeSymbol, displayName, null);
            }
        }

        return null;
    }
}