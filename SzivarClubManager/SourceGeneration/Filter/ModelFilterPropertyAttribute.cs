using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter property for source generation and describes how it maps to SQL filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModelFilterPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the SQL column or expression used when generating filter clauses for this property.
    /// </summary>
    public string DatabaseColumnName { get; }

    /// <summary>
    /// Gets the optional logical group name used to combine related properties (for example Min/Max ranges).
    /// </summary>
    public string PropertyGroupName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelFilterPropertyAttribute"/> class.
    /// </summary>
    /// <param name="databaseColumnName">The SQL column or expression used for generated query fragments.</param>
    /// <param name="propertyGroupName">
    /// Optional logical group name. If omitted, the property name is used as the group key.
    /// </param>
    public ModelFilterPropertyAttribute(string databaseColumnName, string propertyGroupName = "")
    {
        DatabaseColumnName = databaseColumnName;
        PropertyGroupName = propertyGroupName;
    }
}