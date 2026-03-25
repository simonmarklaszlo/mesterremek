using System;

namespace SzivarClubManager.SourceGeneration.Factory;

[AttributeUsage(AttributeTargets.Class)]
public class FactoryAttribute : Attribute
{
    public Type? ModelType { get; }
    public FactoryAttribute(Type? modelType = null) => ModelType = modelType;
}