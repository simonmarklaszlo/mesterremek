using System;

namespace SzivarClubManager.SourceGeneration.Activity;

[AttributeUsage(AttributeTargets.Class)]
public class PageActivityCollectionItemAttribute : Attribute
{
    public string DisplayName { get; }
    public byte OrderGroup { get; }
    public Type ModelType { get; }

    public PageActivityCollectionItemAttribute(string displayName, Type modelType, byte orderGroup = byte.MaxValue)
    {
        DisplayName = displayName;
        OrderGroup = orderGroup;
        ModelType = modelType;
    }
}