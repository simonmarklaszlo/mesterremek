using System;

namespace SzivarClubManager.SourceGeneration.Model;

[AttributeUsage(AttributeTargets.Class)]
public class ModelAttribute : Attribute
{
    public ModelOptions Options { get; }
    public ModelAttribute(ModelOptions options = ModelOptions.None)
    {
        Options = options;
    }
}