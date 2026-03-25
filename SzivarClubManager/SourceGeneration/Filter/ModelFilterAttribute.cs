using System;

namespace SzivarClubManager.SourceGeneration.Filter;

[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterAttribute : Attribute
{
    public Type ModelType { get; }

    public ModelFilterAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}