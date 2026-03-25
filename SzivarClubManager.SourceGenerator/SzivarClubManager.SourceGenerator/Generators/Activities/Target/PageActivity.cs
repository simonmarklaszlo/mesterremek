using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Generators.Activities.Target;

public class PageActivity : SimpleActivity
{
    public INamedTypeSymbol ModelSymbol { get; private set; }

    public PageActivity(INamedTypeSymbol symbol, string displayName, byte orderGroup, INamedTypeSymbol modelSymbol) : base(symbol, displayName, orderGroup)
    {
        ModelSymbol = modelSymbol;
    }


    public string InstanceCreation(string popupService, string factoryProvider) => $"new {Symbol.GlobalName()}({popupService}, {factoryProvider}.GetPageFactory<{ModelSymbol.GlobalName()}>())";

    public new static IncrementalValuesProvider<PageActivity> GetCandidates(IncrementalGeneratorInitializationContext context) => context.SyntaxProvider.ForAttributeWithMetadataName(
        StringReferences.PageActivityCollectionItemAttribute,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) =>
        {
            var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
            var attribute = ctx.Attributes[0];

            string? displayName = null;
            byte? orderGroup = null;
            INamedTypeSymbol? modelSymbol = null;
            if (attribute.ConstructorArguments.Length >= 1 && attribute.ConstructorArguments[0].Value is string displayNameValue) displayName = displayNameValue;
            if (attribute.ConstructorArguments.Length >= 2 && attribute.ConstructorArguments[1].Value is INamedTypeSymbol modelTypeSymbolValue) modelSymbol = modelTypeSymbolValue;
            if (attribute.ConstructorArguments.Length >= 3 && attribute.ConstructorArguments[2].Value is byte orderGroupValue) orderGroup = orderGroupValue;


            return new PageActivity(symbol, displayName ?? symbol.Name, orderGroup ?? byte.MaxValue, modelSymbol!);
        });
}