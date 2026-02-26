using Microsoft.CodeAnalysis;
using SzivarClubManager.SourceGenerator.Pages.Generation;
using SzivarClubManager.SourceGenerator.Pages.Target;

namespace SzivarClubManager.SourceGenerator.Pages;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var vms = PageDataViewModel.GetCandidates(context);

        var vmsCollected = vms.Collect();

        context.RegisterSourceOutput(vmsCollected, static (spc, source) =>
        {
            spc.AddSource(PageDataView.FileName, PageDataView.GenerateSource(source));
        });
    }
}