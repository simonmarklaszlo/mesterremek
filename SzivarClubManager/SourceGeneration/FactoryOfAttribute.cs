using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class)]
public class FactoryOfAttribute : Attribute
{
    public Type ModelType { get; }
    public bool RequiresDatabase { get; }

    public FactoryOfAttribute(Type modelType, bool requiresDatabase)
    {
        ModelType = modelType;
        RequiresDatabase = requiresDatabase;
    }
}