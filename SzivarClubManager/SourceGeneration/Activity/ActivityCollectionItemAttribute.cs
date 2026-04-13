using System;
using SzivarClubManager.SourceGeneration.Generated;

namespace SzivarClubManager.SourceGeneration.Activity;


/// <summary>
/// <para> Adds activity to <see cref="ActivityCollection"/></para>
/// <para> Specifies the display name and order group of an activity.</para>
/// </summary>
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