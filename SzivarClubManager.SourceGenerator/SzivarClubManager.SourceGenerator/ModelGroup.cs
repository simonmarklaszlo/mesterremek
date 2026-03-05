using SzivarClubManager.SourceGenerator.CommonTargets;
using SzivarClubManager.SourceGenerator.CommonTargets.Factory;

namespace SzivarClubManager.SourceGenerator;

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