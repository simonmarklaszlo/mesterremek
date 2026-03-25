using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Generators.Factories.Warnings;

namespace SzivarClubManager.SourceGenerator.Generators.Factories.Target;

public class Factory
{
    public INamedTypeSymbol Symbol { get; }
    public INamedTypeSymbol? ModelSymbol { get; private set; }
    public FactoryType Type { get; private set; } = FactoryType.Unspecified;
    private bool _requiresDatabaseConnection = true;


    public INamedTypeSymbol[] Dependencies { get; private set; } = [];

    public string Name => Symbol.Name;
    public string VariableName => Symbol.PascalCaseName();

    public List<Diagnostic> Diagnostics { get; } = [];
    public bool IsValid => Diagnostics.All(x => x.Severity is not DiagnosticSeverity.Error);

    private Factory(INamedTypeSymbol symbol)
    {
        Symbol = symbol;
    }

    public string InstanceCreation(string dbConnVarName)
    {
        if (_requiresDatabaseConnection)
        {
            if (Dependencies.Length == 0)
            {
                return $"var {VariableName} = new {Symbol.GlobalName()}({dbConnVarName});";
            }

            return $"var {VariableName} = new {Symbol.GlobalName()}({dbConnVarName}, {string.Join(", ", Dependencies.Select(x => x.PascalCaseName()))});";
        }

        if (Dependencies.Length == 0)
        {
            return $"var {VariableName} = new {Symbol.GlobalName()}({string.Join(", ", Dependencies.Select(x => x.PascalCaseName()))});";
        }

        return $"var {VariableName} = new {Symbol.GlobalName()}();";
    }

    public void Initialize(CachedFactorySymbols cachedFactorySymbols)
    {
        var interfaces = Symbol.AllInterfaces;

        if (interfaces.Length == 0)
        {
            Diagnostics.Add(Diagnostic.Create(FactoryWarnings.NoInterfaceError, Symbol.Locations.FirstOrDefault(), Name));
        }

        var modelFactorySymbol = interfaces.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, cachedFactorySymbols.ModelFactoryInterfaceSymbol));
        var helperFactorySymbol = interfaces.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, cachedFactorySymbols.HelperFactoryInterfaceSymbol));
        var pageFactorySymbol = interfaces.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, cachedFactorySymbols.PageFactoryInterfaceSymbol));

        INamedTypeSymbol? modelSymbol = null;
        FactoryType type = FactoryType.Unspecified;
        if (modelFactorySymbol is not null)
        {
            modelSymbol = (INamedTypeSymbol)modelFactorySymbol.TypeArguments[0];
        }

        if (helperFactorySymbol is not null)
        {
            var symbol = (INamedTypeSymbol)helperFactorySymbol.TypeArguments[0];
            if (modelSymbol is not null && !SymbolEqualityComparer.Default.Equals(symbol, modelSymbol))
            {
                Diagnostics.Add(Diagnostic.Create(FactoryWarnings.MultipleModelsError, Symbol.Locations.FirstOrDefault(), Name));
                return;
            }

            modelSymbol = symbol;
            type |= FactoryType.Helper;
        }

        if (pageFactorySymbol is not null)
        {
            var symbol = (INamedTypeSymbol)pageFactorySymbol.TypeArguments[0];
            if (modelSymbol is not null && !SymbolEqualityComparer.Default.Equals(symbol, modelSymbol))
            {
                Diagnostics.Add(Diagnostic.Create(FactoryWarnings.MultipleModelsError, Symbol.Locations.FirstOrDefault(), Name));
                return;
            }

            modelSymbol = symbol;
            type |= FactoryType.Page;
        }

        if (modelSymbol is null)
        {
            var factoryAttribute = Symbol.GetAttributes().First(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, cachedFactorySymbols.FactoryAttributeSymbol));
            if (factoryAttribute.ConstructorArguments.Length == 1 && factoryAttribute.ConstructorArguments[0] is { Value: INamedTypeSymbol modelTypeSymbol })
            {
                modelSymbol = modelTypeSymbol;
            }
            else
            {
                Diagnostics.Add(Diagnostic.Create(FactoryWarnings.UnknownModelError, Symbol.Locations.FirstOrDefault(), Name));
                return;
            }
        }

        if (modelSymbol.AllInterfaces.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.OriginalDefinition, cachedFactorySymbols.ModelInterfaceSymbol)) is null)
        {
            Diagnostics.Add(Diagnostic.Create(FactoryWarnings.ModelIsNotModel, Symbol.Locations.FirstOrDefault(), Name, modelSymbol.Name));
        }

        ModelSymbol = modelSymbol;
        Type = type;

        if (Symbol.Constructors.Length != 1)
        {
            Diagnostics.Add(Diagnostic.Create(FactoryWarnings.WrongNumberOfConstuctors, Symbol.Locations.FirstOrDefault(), Name));
            return;
        }

        var constructor = Symbol.Constructors.First();
        if (!constructor.Parameters.Any(x => SymbolEqualityComparer.Default.Equals(x.Type, cachedFactorySymbols.DatabaseConnectionSymbol)))
        {
            Diagnostics.Add(Diagnostic.Create(FactoryWarnings.NoDatabaseConnection, Symbol.Locations.FirstOrDefault(), Name));
            _requiresDatabaseConnection = false;

            Dependencies = constructor.Parameters
                .Select(x => x.Type as INamedTypeSymbol)
                .Where(x => x is not null)
                .Select(x => x!)
                .ToArray();
        }
        else
        {
            Dependencies = constructor.Parameters
                .Where(x => !SymbolEqualityComparer.Default.Equals(x.Type, cachedFactorySymbols.DatabaseConnectionSymbol))
                .Select(x => x.Type as INamedTypeSymbol)
                .Where(x => x is not null)
                .Select(x => x!)
                .ToArray();
        }
    }

    public static IncrementalValuesProvider<Factory> GetCandidates(IncrementalGeneratorInitializationContext context) => context.SyntaxProvider.ForAttributeWithMetadataName(
        StringReferences.FactoryAttribute,
        static (node, _) => node is ClassDeclarationSyntax,
        static (ctx, _) => new Factory((INamedTypeSymbol)ctx.TargetSymbol)
    );
}