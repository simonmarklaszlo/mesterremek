using System.Linq;
using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Generators.Activities.Generation;
using SzivarClubManager.SourceGenerator.Generators.Activities.Target;

namespace SzivarClubManager.SourceGenerator.Generators.Activities;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var activities = SimpleActivity.GetCandidates(context);
        var pageActivities = PageActivity.GetCandidates(context);

        var combined = activities.Collect()
            .Combine(pageActivities.Collect());

        context.RegisterSourceOutput(combined, static (ctx, source) =>
        {
            var (activities, pageActivities) = source;

            ctx.AddSource(ActivityCollection.FileName, ActivityCollection.GenerateSource(activities.Concat(pageActivities)));
        });
    }
}