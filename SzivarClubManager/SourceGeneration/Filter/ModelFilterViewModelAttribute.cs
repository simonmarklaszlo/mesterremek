using System;

namespace SzivarClubManager.SourceGeneration.Filter;

/// <summary>
/// Marks a filter view model class for source generation.
/// </summary>
/// <remarks>
/// The filter view model generator scans classes annotated with this attribute and emits the
/// corresponding text/state properties, clear commands, apply command, and refresh logic based on
/// the filter model type parameter of the class base type.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ModelFilterViewModelAttribute : Attribute;