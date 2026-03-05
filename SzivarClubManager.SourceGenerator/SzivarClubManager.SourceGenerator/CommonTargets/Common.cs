using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.CommonTargets;

public static class Common
{
    public static IncrementalValuesProvider<T> GetCandidates<T>(IncrementalGeneratorInitializationContext context, Func<GeneratorSyntaxContext, T?> isTarget) =>
        context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is TypeDeclarationSyntax,
                (ctx, _) => isTarget(ctx)
            )
            .Where(t => t is not null)
            .Select((t, _) => t!);
}