using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter view-model class as a target for filter view-model source generation.
/// </summary>
/// <remarks>
/// Apply this attribute to a <see langword="partial"/> class that inherits
/// <c>FilterViewModel&lt;TFilter, TModel&gt;</c>.
/// The generator resolves <c>TFilter</c> from the base type and emits the generated
/// text/state properties and filter commands into the decorated class.
/// <para>
/// Predicate methods can be mapped to filter properties by using
/// <see cref="PredicateOfPropertyAttribute"/> on methods in the same class.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterViewModelAttribute : Attribute;