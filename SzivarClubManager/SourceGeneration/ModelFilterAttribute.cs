using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterAttribute : Attribute
{
    public Type ModelType { get; }

    public ModelFilterAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}