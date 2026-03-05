using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PageActivityCollectionItemAttribute : ActivityCollectionItemAttribute
{
    public Type ModelType { get; }
    public PageActivityCollectionItemAttribute(Type modelType, string displayName, byte orderGroup = byte.MaxValue) : base(displayName, orderGroup)
    {
        ModelType = modelType;
    }
}