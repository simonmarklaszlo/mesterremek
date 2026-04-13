using System;
using SzivarClubManager.SourceGeneration.Generated;

namespace SzivarClubManager.SourceGeneration.Activity;

/// <summary>
/// <para> Adds page activity to <see cref="ActivityCollection"/></para>
/// <para> Specifies the display name, model type, and order group of a page activity.</para>
/// </summary>
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