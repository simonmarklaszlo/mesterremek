using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class PageActivityAttribute : Attribute
{
    public bool GenerateActivity { get; }

    public PageActivityAttribute(bool generateActivity = true)
    {
        GenerateActivity = generateActivity;
    }
}