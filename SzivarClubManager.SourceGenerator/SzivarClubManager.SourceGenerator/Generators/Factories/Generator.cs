using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Factories.Generation;
using SzivarClubManager.SourceGenerator.Generators.Factories.Target;
using SzivarClubManager.SourceGenerator.Generators.Factories.Warnings;

namespace SzivarClubManager.SourceGenerator.Generators.Factories;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var cache = context.CompilationProvider.Select(static (compilation, _) => CachedFactorySymbols.Load(compilation));
        var factories = Factory.GetCandidates(context);

        var combined = factories.Collect()
            .Combine(cache);

        context.RegisterSourceOutput(combined, RegisterFactories);
    }

    private static void RegisterFactories(SourceProductionContext ctx, (ImmutableArray<Factory> Left, (CachedFactorySymbols? cachedSymbols, List<string> missingSymbols) Right) source)
    {
        var (factories, cache) = source;

        if (cache.cachedSymbols is null)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(FactoryWarnings.MissingSymbols, Location.None, string.Join(", ", cache.missingSymbols)));
            ctx.AddSource(FactoryProvider.FileName, FactoryProvider.GenerateSource(Enumerable.Empty<Factory>().ToImmutableArray()));
            return;
        }


        foreach (var factory in factories)
        {
            factory.Initialize(cache.cachedSymbols);
            foreach (var diagnostic in factory.Diagnostics)
            {
                ctx.ReportDiagnostic(diagnostic);
            }
        }

        ctx.AddSource(FactoryProvider.FileName, FactoryProvider.GenerateSource(factories));
    }
}