using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator.Generators.Factories.Warnings;

public static class FactoryWarnings
{
    public static DiagnosticDescriptor NoInterfaceError { get; } = new(
        id: "SZIVAR-F01",
        title: "Missing interfaces",
        messageFormat: "Factory {0} does not implement any factory interfaces",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor MultipleModelsError { get; } = new(
        id: "SZIVAR-F02",
        title: "Interfaces have multiple model types",
        messageFormat: "Factory {0} has to many model types in implemented interfaces",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor UnknownModelError { get; } = new(
        id: "SZIVAR-F03",
        title: "Factory model could not be inferred",
        messageFormat: "Factory {0}'s model could not be inferred",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor WrongNumberOfConstuctors { get; } = new(
        id: "SZIVAR-F04",
        title: "Wrong number of constructors",
        messageFormat: "Factory {0} has more or less than one constructor",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NoDatabaseConnection { get; } = new(
        id: "SZIVAR-F05",
        title: "DatabaseConnection missing",
        messageFormat: "Factory {0}'s constructor does not contain a DatabaseConnection parameter",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor MissingSymbols { get; } = new(
        id: "SZIVAR-F06",
        title: "Missing critical symbols",
        messageFormat: "Unable to find symbols : {0}",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor ModelIsNotModel { get; } = new(
        id: "SZIVAR-F07",
        title: "Model is not IModel",
        messageFormat: "Factory {0} model {1} does not implement IModel",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );
}