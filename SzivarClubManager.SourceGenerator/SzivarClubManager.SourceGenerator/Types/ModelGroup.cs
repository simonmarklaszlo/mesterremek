using SzivarClubManager.SourceGenerator.Targets;
using SzivarClubManager.SourceGenerator.Targets.Factory;

namespace SzivarClubManager.SourceGenerator.Types;

public readonly struct ModelGroup
{
    public Model Model { get; }
    public Factory? Factory { get; }

    public ModelGroup(Model model, Factory? factory)
    {
        Model = model;
        Factory = factory;
    }
}