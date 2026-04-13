using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter class as a source-generation target and binds it to a model type.
/// </summary>
/// <remarks>
/// Apply this attribute to classes that declare <see cref="ModelFilterPropertyAttribute"/> members.
/// The filter source generator reads <see cref="ModelType"/> from the first constructor argument.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterAttribute : Attribute
{
    /// <summary>
    /// Gets the model type the decorated filter class targets.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelFilterAttribute"/> class.
    /// </summary>
    /// <param name="modelType">
    /// The model type used by source generation, for example <c>typeof(Brand)</c>.
    /// </param>
    public ModelFilterAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}