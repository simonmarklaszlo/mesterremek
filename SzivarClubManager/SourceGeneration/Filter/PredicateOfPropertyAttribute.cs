using System;
using SzivarClubManager.ViewModels.Activities.Page.Filter;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a predicate method as the validator for a specific filter-model property.
/// </summary>
/// <remarks>
/// This attribute is consumed by the source generator to bind a predicate method to the
/// property identified by <see cref="PropertyName"/>.
/// <para>
/// For string properties, the predicate result determines whether the field state is
/// <see cref="FieldState.Valid"/> or <see cref="FieldState.Invalid"/>.
/// </para>
/// <para>
/// For integer properties, the predicate result determines whether the field state is
/// <see cref="FieldState.Valid"/> or <see cref="FieldState.Warning"/>. Integer properties become
/// <see cref="FieldState.Invalid"/> when the input string cannot be parsed as an integer.
/// </para>
/// <para>
/// Typical usage:
/// <code>
/// [PredicateOfProperty(nameof(BrandFilter.Name))]
/// private static bool StringPredicate(string text) => !text.Contains('"');
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PredicateOfPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the filter property that the annotated predicate method validates.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateOfPropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the filter property associated with the predicate method.</param>
    public PredicateOfPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}