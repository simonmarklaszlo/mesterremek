using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Factories.Generation;
using SzivarClubManager.SourceGenerator.Targets.Factory;

namespace SzivarClubManager.SourceGenerator.Factories;

[Generator]
public class Generator : IIncrementalGenerator
{
    //TODO : warn on type collision
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var factories = Factory.GetCandidates(context);
        var fakeFactories = FakeFactory.GetCandidates(context);

        var combined = factories.Collect()
            .Combine(fakeFactories.Collect());

        context.RegisterSourceOutput(combined, static (ctx, source) =>
        {
            var (factories, fakeFactories) = source;


            ctx.AddSource(
                FactoryProvider.FileName,
                FactoryProvider.GenerateSource(factories, fakeFactories)
            );
        });
    }
}