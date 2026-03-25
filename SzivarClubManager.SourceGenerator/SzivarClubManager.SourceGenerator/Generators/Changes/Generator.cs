using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Changes.Generation;
using SzivarClubManager.SourceGenerator.Generators.Changes.Target;
using SzivarClubManager.SourceGenerator.Generators.Factories.Target;
using SzivarClubManager.SourceGenerator.Generators.Factories.Warnings;

namespace SzivarClubManager.SourceGenerator.Generators.Changes;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = Model.GetCandidates(context);
        var factories = Factory.GetCandidates(context);
        var factoryCache = context.CompilationProvider.Select(static (compilation, _) => CachedFactorySymbols.Load(compilation));


        var factoriesCombined = models.Collect()
            .Combine(factories.Collect())
            .Combine(factoryCache);

        context.RegisterSourceOutput(factoriesCombined, (ctx, source) => RegisterChanges(ctx, new Data(source)));
    }

    private static void RegisterChanges(SourceProductionContext ctx, Data data)
    {
        if (data.CachedFactorySymbols is null)
        {
            if (data.FactoryMissingSymbols.Count > 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(FactoryWarnings.MissingSymbols, Location.None, string.Join(", ", data.FactoryMissingSymbols)));
            }

            ctx.AddSource(Generation.Changes.FileName, Generation.Changes.GenerateSource(ImmutableArray<Model>.Empty));
            ctx.AddSource(ModelRowBackgroundConverter.FileName, ModelRowBackgroundConverter.GenerateSource(ImmutableArray<Model>.Empty));
            ctx.AddSource(ChangesActivityViewModel.FileName, ChangesActivityViewModel.GenerateSource(ImmutableArray<Model>.Empty));
            return;
        }

        foreach (var factory in data.Factories)
        {
            factory.Initialize(data.CachedFactorySymbols);
            foreach (var diagnostic in factory.Diagnostics)
            {
                ctx.ReportDiagnostic(diagnostic);
            }
        }

        foreach (var model in data.Models)
        {
            model.Initialize(data.Factories);
            foreach (var diagnostic in model.Diagnostics)
            {
                ctx.ReportDiagnostic(diagnostic);
            }
        }

        var models = data.Models
            .Where(x => x.Options is not Model.ModelOptions.Unchangeable)
            .ToImmutableArray();

        ctx.AddSource(Generation.Changes.FileName, Generation.Changes.GenerateSource(models));
        ctx.AddSource(ModelRowBackgroundConverter.FileName, ModelRowBackgroundConverter.GenerateSource(models));
        ctx.AddSource(ChangesActivityViewModel.FileName, ChangesActivityViewModel.GenerateSource(models));
    }

    private readonly ref struct Data
    {
        public ImmutableArray<Factory> Factories { get; }
        public CachedFactorySymbols? CachedFactorySymbols { get; }
        public List<string> FactoryMissingSymbols { get; }

        public ImmutableArray<Model> Models { get; }

        public Data(((ImmutableArray<Model> Left, ImmutableArray<Factory> Right) Left, (CachedFactorySymbols? cachedSymbols, List<string> missingSymbols) Right) data)
        {
            Models = data.Left.Left;

            Factories = data.Left.Right;
            CachedFactorySymbols = data.Right.cachedSymbols;
            FactoryMissingSymbols = data.Right.missingSymbols;
        }
    }
}