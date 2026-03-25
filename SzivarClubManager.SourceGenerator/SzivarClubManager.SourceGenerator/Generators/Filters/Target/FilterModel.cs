using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SzivarClubManager.SourceGenerator.Generators.Filters.Target;

public class FilterModel
{
    public INamedTypeSymbol FilterType { get; }
    public INamedTypeSymbol ModelType { get; }

    public Dictionary<string, Property[]> PropertyGroups { get; }
    public IEnumerable<Property> Properties => field ??= PropertyGroups.Values.SelectMany(x => x).ToArray();

    public FilterModel(INamedTypeSymbol filterType, INamedTypeSymbol modelType, Dictionary<string, Property[]> propertyGroups)
    {
        FilterType = filterType;
        ModelType = modelType;
        PropertyGroups = propertyGroups;
    }

    public static IncrementalValuesProvider<FilterModel> GetCandidates(IncrementalGeneratorInitializationContext context) =>
        context.SyntaxProvider.ForAttributeWithMetadataName(
            StringReferences.FilterAttribute,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) =>
            {
                var classSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
                var attribute = ctx.Attributes[0];

                var modelType = (INamedTypeSymbol)attribute
                    .ConstructorArguments[0]
                    .Value!;

                return CreateFilterModel(classSymbol, modelType);
            });

    private static FilterModel CreateFilterModel(INamedTypeSymbol filterType, INamedTypeSymbol modelType)
    {
        var properties = filterType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == StringReferences.FilterPropertyAttribute))
            .Select(x =>
            {
                var (dbCol, groupName) = GetPropertyGroupAndDatabaseColumn(x);
                return new { DatabaseColumn = dbCol, GroupName = groupName, PropertySymbol = x };
            })
            .GroupBy(x => x.GroupName)
            .ToDictionary(x => x.Key, x => x.Select(y => new Property(y.PropertySymbol, y.DatabaseColumn)).ToArray());

        return new FilterModel(filterType, modelType, properties);
    }

    private static (string DatabaseColumn, string GroupName) GetPropertyGroupAndDatabaseColumn(IPropertySymbol propertySymbol)
    {
        var attribute = propertySymbol
            .GetAttributes()
            .First(a => a.AttributeClass?.ToDisplayString() == StringReferences.FilterPropertyAttribute);

        string dbCol = (string)attribute.ConstructorArguments[0].Value!;

        if (attribute.ConstructorArguments.Length == 1 || attribute.ConstructorArguments[1].Value is not string groupName || string.IsNullOrWhiteSpace(groupName))
        {
            return (dbCol, propertySymbol.Name);
        }

        return (dbCol, groupName);
    }
}

public class Property
{
    public string Name { get; }
    public string? DbColumnName { get; }
    public PropertyTypes Type { get; }
    public bool IsSupported => Type is not PropertyTypes.Unsupported;

    public Property(IPropertySymbol propertySymbol, string? dbColumnName)
    {
        Name = propertySymbol.Name;


        DbColumnName = dbColumnName;

        // Handle Nullable<T> (value types like int?, double?)
        var typeSymbol = propertySymbol.Type;
        // unwrap T if necessary
        if (typeSymbol is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) typeSymbol = namedType.TypeArguments[0];
        Type = typeSymbol.SpecialType switch
        {
            SpecialType.System_String => PropertyTypes.String,
            SpecialType.System_Int32 => PropertyTypes.Int,
            SpecialType.System_Double => PropertyTypes.Double,
            _ => PropertyTypes.Unsupported
        };
    }
}

public enum PropertyTypes
{
    Unsupported,
    String,
    Int,
    Double
}