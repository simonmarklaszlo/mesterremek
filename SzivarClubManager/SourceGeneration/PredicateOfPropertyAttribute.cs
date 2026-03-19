using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PredicateOfPropertyAttribute : Attribute
{
    public string PropertyName { get; }

    public PredicateOfPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}