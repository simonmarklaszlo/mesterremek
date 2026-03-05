using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class FactoryOfAttribute : Attribute
{
    public Type ModelType { get; }
    public Type[] Dependencies { get; }

    public FactoryOfAttribute(Type modelType, params Type[] dependencies)
    {
        ModelType = modelType;
        Dependencies = dependencies;
    }
}