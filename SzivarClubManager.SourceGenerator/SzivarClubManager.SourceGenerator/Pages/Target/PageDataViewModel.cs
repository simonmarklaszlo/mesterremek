using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Targets;

namespace SzivarClubManager.SourceGenerator.Pages.Target;

public record PageDataViewModel(INamedTypeSymbol ViewModelSymbol, INamedTypeSymbol ModelSymbol)
{
    public string Name => ViewModelSymbol.Name;
    public string ModelName => ModelSymbol.Name;

    private const string PageDataViewModelDeclaration = "SzivarClubManager.ViewModels.Activities.Page.Data.PageDataViewModel`1";

    public static IncrementalValuesProvider<PageDataViewModel> GetCandidates(IncrementalGeneratorInitializationContext context) => Common.GetCandidates(context, IsTarget);

    private static PageDataViewModel? IsTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl) return null;
        if (context.SemanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol namedTypeSymbol) return null;

        var modelSymbol = GetModelSymbol(namedTypeSymbol, context.SemanticModel.Compilation);
        if (modelSymbol is null) return null;

        return new PageDataViewModel(namedTypeSymbol, modelSymbol);
    }

    private static INamedTypeSymbol? GetModelSymbol(INamedTypeSymbol symbol, Compilation compilation)
    {
        var targetBase = compilation.GetTypeByMetadataName(PageDataViewModelDeclaration);
        if (targetBase is null) return null;

        var current = symbol.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, targetBase))
            {
                return current.TypeArguments[0] as INamedTypeSymbol;
            }

            current = current.BaseType;
        }

        return null;
    }
}