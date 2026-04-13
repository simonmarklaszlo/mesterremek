using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter model property for the filter source generator and maps it to a database column or SQL expression.
/// </summary>
/// <remarks>
/// Properties that share the same <c>propertyGroupName</c> are generated as one logical filter entry.
/// When <c>propertyGroupName</c> is omitted or empty, the property name is used as the group key.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class ModelFilterPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the database column name or SQL expression used when generating filter predicates.
    /// </summary>
    public string DatabaseColumnName { get; }

    /// <summary>
    /// Gets the logical filter group name used to combine multiple properties into a single generated filter key.
    /// </summary>
    public string PropertyGroupName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelFilterPropertyAttribute"/> class.
    /// </summary>
    /// <param name="databaseColumnName">
    /// The target database column name or SQL expression (for example, <c>name</c> or <c>ST_Y(location)</c>).
    /// </param>
    /// <param name="propertyGroupName">
    /// Optional logical group name for related properties (for example min/max range pairs).
    /// If <see langword="null"/>, empty, or whitespace, the annotated property name is used.
    /// </param>
    public ModelFilterPropertyAttribute(string databaseColumnName, string propertyGroupName = "")
    {
        DatabaseColumnName = databaseColumnName;
        PropertyGroupName = propertyGroupName;
    }
}