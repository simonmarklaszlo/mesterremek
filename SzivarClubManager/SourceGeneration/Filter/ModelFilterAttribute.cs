using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter class for source generation and binds it to a model type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterAttribute : Attribute
{
    /// <summary>
    /// Gets the model type that this filter targets.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelFilterAttribute"/> class.
    /// </summary>
    /// <param name="modelType">The model type the annotated filter class belongs to.</param>
    public ModelFilterAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}