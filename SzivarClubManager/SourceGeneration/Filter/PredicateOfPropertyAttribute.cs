using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Binds a predicate method to a filter property handled by generated filter view model code.
/// </summary>
/// <remarks>
/// Apply this to methods inside a class marked with <see cref="ModelFilterViewModelAttribute"/>.
/// The source generator resolves <see cref="PropertyName"/> and uses the annotated method as the
/// validator in generated <c>Check*State</c> methods for that property. The attribute can be applied
/// multiple times to map one method to multiple properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PredicateOfPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the target filter property name this predicate validates.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateOfPropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">
    /// The exact filter property name (typically via <c>nameof(FilterType.Property)</c>) that should
    /// use the annotated method as its validation predicate in generated code.
    /// </param>
    public PredicateOfPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
