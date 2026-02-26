using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class FactoryOfAttribute : Attribute
{
    public Type ModelType { get; }
    public bool RequiresDatabase { get; }
    public Type[] Dependencies { get; }

    public FactoryOfAttribute(Type modelType, bool requiresDatabase, params Type[] dependencies)
    {
        ModelType = modelType;
        RequiresDatabase = requiresDatabase;
        Dependencies = dependencies;
    }
}