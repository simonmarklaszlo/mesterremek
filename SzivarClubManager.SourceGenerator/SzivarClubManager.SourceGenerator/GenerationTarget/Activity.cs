using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.GenerationTarget;

public class Activity : ITarget
{
    public string DisplayName { get; }
    public string ViewModelType { get; }

    public Activity(string displayName, string viewModelType)
    {
        DisplayName = displayName;
        ViewModelType = viewModelType;
    }

    public virtual string InstanceCreation(string dataSourceVariableName) =>
        $"new {ViewModelType}({dataSourceVariableName})";

    public static Activity? IsTarget(GeneratorSyntaxContext context)
    {
        return null;
    }
}