using Microsoft.CodeAnalysis;

namespace SzivarClubManager.SourceGenerator;

public static class Warnings
{
    public static DiagnosticDescriptor NoFactoryWarning { get; } = new(
        id: "SZIVAR001",
        title: "Missing database factory",
        messageFormat: "No database factory found for {0}",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NoFakeFactoryWarning { get; } = new(
        id: "SZIVAR002",
        title: "Missing fake factory",
        messageFormat: "No fake factory found for {0}",
        category: "SzivarClubManagerGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );
}