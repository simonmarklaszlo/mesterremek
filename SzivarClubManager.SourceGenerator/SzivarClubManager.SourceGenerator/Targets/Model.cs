using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Targets;

public class Model
{
    public string Name => ModelSymbol.Name;
    public INamedTypeSymbol ModelSymbol { get; }

    private const string ModelNamespace = "SzivarClubManager.Models";
    private const string IModelInterface = "SzivarClubManager.Models.IModel";

    public Model(INamedTypeSymbol modelSymbol)
    {
        ModelSymbol = modelSymbol;
    }

    public static IncrementalValuesProvider<Model> GetCandidates(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is TypeDeclarationSyntax,
                (ctx, _) => IsTarget(ctx)
            )
            .Where(t => t is not null)
            .Select((t, _) => t!);

    private static Model? IsTarget(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDecl) return null;
        if (context.SemanticModel.GetDeclaredSymbol(typeDecl) is not INamedTypeSymbol namedTypeSymbol) return null;

        if (namedTypeSymbol.ContainingNamespace?.ToDisplayString() != ModelNamespace) return null; // namespace check

        var interfaceSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(IModelInterface);
        if (interfaceSymbol is null) return null;
        if (!namedTypeSymbol.AllInterfaces.Contains(interfaceSymbol)) return null; // interface check

        return new Model(namedTypeSymbol);
    }
}