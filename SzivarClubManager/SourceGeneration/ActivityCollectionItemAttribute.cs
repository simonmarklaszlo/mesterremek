using System;

namespace SzivarClubManager.SourceGeneration;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ActivityCollectionItemAttribute : Attribute
{
    public string DisplayName { get; }
    public byte OrderGroup { get; }
    public ActivityCollectionItemAttribute(string displayName, byte orderGroup = byte.MaxValue)
    {
        DisplayName = displayName;
        OrderGroup = orderGroup;
    }
}