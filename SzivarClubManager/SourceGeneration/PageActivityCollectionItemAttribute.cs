using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PageActivityCollectionItemAttribute : ActivityCollectionItemAttribute
{
    public Type ModelType { get; }
    public PageActivityCollectionItemAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}