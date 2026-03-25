using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SzivarClubManager.SourceGenerator.Generators.Filters.Target;

namespace SzivarClubManager.SourceGenerator.Generators.FilterViewModels.Target;

public class FilterViewModel
{
    public INamedTypeSymbol ViewModelSymbol { get; }
    public INamedTypeSymbol FilterSymbol { get; }
    public Dictionary<string, IMethodSymbol> Predicates { get; }


    public FilterViewModel(INamedTypeSymbol viewModelSymbol, INamedTypeSymbol filterSymbol, Dictionary<string, IMethodSymbol> predicates)
    {
        ViewModelSymbol = viewModelSymbol;
        FilterSymbol = filterSymbol;
        Predicates = predicates;
    }

    public string? TryGetMethodName(Property prop)
    {
        if (Predicates.TryGetValue(prop.Name, out var methodSymbol))
        {
            return methodSymbol.Name;
        }

        return null;
    }

    public static IncrementalValuesProvider<FilterViewModel> GetCandidates(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
            StringReferences.FilterViewModelAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => Transform(ctx));

    private static FilterViewModel Transform(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol { BaseType: { } baseType } vmSymbol) throw new Exception();
        if (baseType.TypeArguments.Length != 2) throw new Exception();

        var filterSymbol = (INamedTypeSymbol)baseType.TypeArguments[0];

        var predicates = vmSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .SelectMany(m => m.GetAttributes()
                .Where(a => a.AttributeClass?.ToDisplayString() == StringReferences.FilterPredicateOfProperty)
                .Select(a => new
                {
                    PropertyName = (string)a.ConstructorArguments[0].Value!,
                    Method = m
                }))
            .ToDictionary(
                x => x.PropertyName,
                x => x.Method
            );

        return new FilterViewModel(vmSymbol, filterSymbol, predicates);
    }
}