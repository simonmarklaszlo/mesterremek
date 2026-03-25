using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Generators.Activities.Target;

public class SimpleActivity
{
    public INamedTypeSymbol Symbol { get; }
    public string DisplayName { get; }
    public byte OrderGroup { get; }

    public SimpleActivity(INamedTypeSymbol symbol, string displayName, byte orderGroup)
    {
        Symbol = symbol;
        DisplayName = displayName;
        OrderGroup = orderGroup;
    }


    public string InstanceCreation(string popupServiceVarName) => $"new {Symbol.GlobalName()}({popupServiceVarName})";

    public static IncrementalValuesProvider<SimpleActivity> GetCandidates(IncrementalGeneratorInitializationContext context) => context.SyntaxProvider.ForAttributeWithMetadataName(
        StringReferences.ActivityCollectionItemAttribute,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) =>
        {
            var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
            var attribute = ctx.Attributes[0];

            string? displayName = null;
            byte? orderGroup = null;
            if (attribute.ConstructorArguments.Length >= 1 && attribute.ConstructorArguments[0].Value is string displayNameValue) displayName = displayNameValue;
            if (attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is byte orderGroupValue) orderGroup = orderGroupValue;

            return new SimpleActivity(symbol, displayName ?? symbol.Name, orderGroup ?? byte.MaxValue);
        });
}