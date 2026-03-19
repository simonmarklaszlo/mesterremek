using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Targets;

namespace SzivarClubManager.SourceGenerator.Generators.Changes.Target;

public class PageActivityViewModel
{
    public INamedTypeSymbol ActivitySymbol { get; }
    public INamedTypeSymbol ModelSymbol { get; }

    public PageActivityViewModel(INamedTypeSymbol activitySymbol, INamedTypeSymbol modelSymbol)
    {
        ActivitySymbol = activitySymbol;
        ModelSymbol = modelSymbol;
    }

    public static IncrementalValuesProvider<PageActivityViewModel> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static PageActivityViewModel? IsTarget(GeneratorSyntaxContext context)
    {
        const string pageActivityViewModel = "SzivarClubManager.ViewModels.Activities.Page.Wrapper.PageActivityViewModel`5";


        if (context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node) is not INamedTypeSymbol typeSymbol) return null;
        var pageActivityViewModelSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(pageActivityViewModel);
        if (pageActivityViewModelSymbol is null) return null;

        if (!typeSymbol.Name.EndsWith("ActivityViewModel")) return null;

        if (typeSymbol.BaseType is null) return null;

        if (!SymbolEqualityComparer.Default.Equals(typeSymbol.BaseType.OriginalDefinition, pageActivityViewModelSymbol)) return null;

        if (typeSymbol.BaseType.TypeArguments.Length < 1 || typeSymbol.BaseType.TypeArguments[0] is not INamedTypeSymbol modelSymbol) return null;

        return new PageActivityViewModel(typeSymbol, modelSymbol);
    }
}