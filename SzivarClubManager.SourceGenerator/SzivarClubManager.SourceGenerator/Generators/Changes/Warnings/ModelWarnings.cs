using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.Generators.Changes.Warnings;

public static class ModelWarnings
{
    public static DiagnosticDescriptor NoFactoryFound { get; } = new(
        id: "SZIVAR-M01",
        title: "Missing factory",
        messageFormat: "Model {0} does not have a factory",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor MissingSymbols { get; } = new(
        id: "SZIVAR-M02",
        title: "Missing critical symbols",
        messageFormat: "Unable to find symbols : {0}",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}