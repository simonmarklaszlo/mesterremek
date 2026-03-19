using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Factories.Generation;
using SzivarClubManager.SourceGenerator.Targets.Factory;

namespace SzivarClubManager.SourceGenerator.Generators.Factories;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var factories = Factory.GetCandidates(context);

        context.RegisterSourceOutput(factories.Collect(), static (ctx, source) =>
        {
            ctx.AddSource(
                FactoryProvider.FileName,
                FactoryProvider.GenerateSource(source)
            );
        });
    }
}