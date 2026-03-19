using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Property)]
public class ModelFilterPropertyAttribute : Attribute
{
    public string DatabaseColumnName { get; }
    public string PropertyGroupName { get; }

    public ModelFilterPropertyAttribute(string databaseColumnName, string propertyGroupName = "")
    {
        DatabaseColumnName = databaseColumnName;
        PropertyGroupName = propertyGroupName;
    }
}