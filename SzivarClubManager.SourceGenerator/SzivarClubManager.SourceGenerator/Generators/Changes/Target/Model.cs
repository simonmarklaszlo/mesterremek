using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Generators.Changes.Warnings;
using SzivarClubManager.SourceGenerator.Generators.Factories.Target;

namespace SzivarClubManager.SourceGenerator.Generators.Changes.Target;

public class Model
{
    public string Name => Symbol.Name;
    public INamedTypeSymbol Symbol { get; }
    public ModelOptions Options { get; }
    public Factory? Factory { get; private set; }
    public List<Diagnostic> Diagnostics { get; } = [];
    public bool IsValid => Diagnostics.All(x => x.Severity is not DiagnosticSeverity.Error);

    private Model(INamedTypeSymbol symbol, ModelOptions options)
    {
        Symbol = symbol;
        Options = options;
    }

    public void Initialize(ImmutableArray<Factory> factories)
    {
        Factory = factories.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.ModelSymbol, Symbol));

        if (Factory is null)
        {
            Diagnostics.Add(Diagnostic.Create(ModelWarnings.NoFactoryFound, Symbol.Locations.FirstOrDefault(), Name));
        }
    }

    public static IncrementalValuesProvider<Model> GetCandidates(IncrementalGeneratorInitializationContext context) => context.SyntaxProvider.ForAttributeWithMetadataName(
        StringReferences.ModelAttribute,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) =>
        {
            var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
            var options = ModelOptions.None;
            var attribute = ctx.Attributes[0];

            if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0] is { Value: int intModelValue })
            {
                options = (ModelOptions)intModelValue;
            }

            return new Model(symbol, options);
        });

    [Flags]
    public enum ModelOptions
    {
        None = 0,
        Unchangeable = 1,
    }
}