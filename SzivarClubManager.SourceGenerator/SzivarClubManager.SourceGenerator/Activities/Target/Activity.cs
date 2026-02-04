using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.Activities.Target;

public class Activity : ITarget
{
    public string DisplayName { get; }
    public string ViewModelType { get; }

    public Activity(string displayName, string viewModelType)
    {
        DisplayName = displayName;
        ViewModelType = viewModelType;
    }

    public string InstanceCreation() => $"new {ViewModelType}()";

    public static Activity? IsTarget(GeneratorSyntaxContext context)
    {
        return null;
    }
}