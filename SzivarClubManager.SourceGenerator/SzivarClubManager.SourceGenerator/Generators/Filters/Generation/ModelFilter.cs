using System.Text;
using SzivarClubManager.SourceGenerator.Generators.Filters.Target;

namespace SzivarClubManager.SourceGenerator.Generators.Filters.Generation;

public static class ModelFilter
{
    private const string FilterValueHandler = "global::SzivarClubManager.Datasources.Database.Filters.FilterValueHandler";
    public static string GetFileName(FilterModel model) => $"{model.FilterType.Name}.g.cs";

    public static string GenerateSource(FilterModel filter)
    {
        StringBuilder sb = new();
        sb.AppendLine("// auto-generated");
        sb.AppendLine($"namespace {filter.FilterType.ContainingNamespace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {filter.FilterType.Name}");
        sb.AppendLine("{");
        GenerateIsEmpty(sb, filter);
        sb.AppendLine();
        GenerateToString(sb, filter);
        sb.AppendLine();
        GenerateCopyTo(sb, filter);
        sb.AppendLine();
        GenerateConstructParameterizedQuery(sb, filter);
        sb.AppendLine();
        GenerateAddParameters(sb, filter);
        sb.AppendLine();
        GenerateParse(sb, filter);
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void GenerateIsEmpty(StringBuilder sb, FilterModel filter)
    {
        sb.Append("    public bool IsEmpty => ");
        bool anyBefore = false;
        foreach (var pair in filter.PropertyGroups)
        {
            if (anyBefore)
            {
                sb.AppendLine(" && ");
                sb.Append("                           "); // indent for "    public bool IsEmpty => "
            }

            if (pair.Value.Length == 1)
            {
                sb.Append($"{pair.Value[0].Name} is null");
            }
            else
            {
                sb.Append($"{pair.Value[0].Name} is null && {pair.Value[1].Name} is null");
            }

            anyBefore = true;
        }

        if (anyBefore) sb.AppendLine(";");
        else sb.AppendLine("true;");
    }

    private static void GenerateToString(StringBuilder sb, FilterModel filter)
    {
        sb.AppendLine("    public override string ToString()");
        sb.AppendLine("    {");
        sb.AppendLine("        global::System.Text.StringBuilder sb = new();");
        sb.AppendLine();
        sb.AppendLine("        bool valueBefore = false;");
        sb.AppendLine();
        foreach (var pair in filter.PropertyGroups)
        {
            sb.AppendLine($"        if ({pair.Value[0].Name} is not null)");
            sb.AppendLine("        {");
            if (pair.Value.Length == 1)
            {
                sb.AppendLine("            if (valueBefore) sb.Append(' ');");
                sb.AppendLine("            else valueBefore = true;");
                if (pair.Value[0].Type is PropertyTypes.String)
                {
                    sb.AppendLine($"            sb.Append(\"{pair.Value[0].Name}=\").Append({FilterValueHandler}.ToDisplayString({pair.Value[0].Name}));");
                }
                else
                {
                    sb.AppendLine($"            sb.Append(\"{pair.Value[0].Name}=\").Append({pair.Value[0].Name}.ToString());");
                }
            }
            else
            {
                sb.AppendLine("            if (valueBefore) sb.Append(' ');");
                sb.AppendLine("            else valueBefore = true;");
                sb.AppendLine();
                sb.AppendLine($"            if ({pair.Value[0].Name} == {pair.Value[1].Name})");
                sb.AppendLine("            {");
                sb.AppendLine($"                sb.Append(\"{pair.Key}=\").Append({pair.Value[0].Name}.ToString());");
                sb.AppendLine("            }");
                sb.AppendLine($"            else");
                sb.AppendLine("            {");
                sb.AppendLine($"                sb.Append(\"{pair.Key}=[\").Append({pair.Value[0].Name}.ToString()).Append(\"..\").Append({pair.Value[1].Name}.ToString()).Append(']');");
                sb.AppendLine("            }");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
    }

    private static void GenerateCopyTo(StringBuilder sb, FilterModel filter)
    {
        sb.AppendLine($"    public void CopyTo(global::SzivarClubManager.Datasources.Database.Filters.IFilter<{filter.ModelType.GlobalName()}> other)");
        sb.AppendLine("    {");
        sb.AppendLine($"        if (other is not {filter.FilterType.GlobalName()} otherFilter) throw new global::System.ArgumentException(\"Other filter is not of type ShopFilter\");");
        sb.AppendLine();
        foreach (Property prop in filter.Properties)
        {
            sb.AppendLine($"        otherFilter.{prop.Name} = {prop.Name};");
        }

        sb.AppendLine("    }");
    }

    private static void GenerateConstructParameterizedQuery(StringBuilder sb, FilterModel filter)
    {
        sb.AppendLine("    public string ConstructParameterizedQuery()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (IsEmpty) return string.Empty;");
        sb.AppendLine();
        sb.AppendLine("        global::System.Text.StringBuilder sb = new();");
        sb.AppendLine();
        sb.AppendLine("        bool valueBefore = false;");
        sb.AppendLine();
        sb.AppendLine("        sb.Append(\"WHERE \");");
        sb.AppendLine();
        foreach (var pair in filter.PropertyGroups)
        {
            sb.AppendLine($"        if ({pair.Value[0].Name} is not null)");
            sb.AppendLine("        {");
            if (pair.Value.Length == 1)
            {
                sb.AppendLine("            if (valueBefore) sb.Append(\" AND \");");
                sb.AppendLine("            else valueBefore = true;");
                if (pair.Value[0].Type is PropertyTypes.String)
                {
                    sb.AppendLine($"            sb.Append(\"{pair.Value[0].DbColumnName} LIKE '%' || @{pair.Value[0].Name} || '%'\");");
                }
                else
                {
                    sb.AppendLine($"            sb.Append(\"{pair.Value[0].DbColumnName} = @{pair.Value[0].Name}\");");
                }
            }
            else
            {
                sb.AppendLine("            if (valueBefore) sb.Append(\" AND \");");
                sb.AppendLine("            else valueBefore = true;");
                sb.AppendLine();
                sb.AppendLine($"            if ({pair.Value[0].Name} == {pair.Value[1].Name} || {pair.Value[1].Name} is null)");
                sb.AppendLine("            {");
                sb.AppendLine($"                sb.Append(\"{pair.Value[0].DbColumnName} = @{pair.Key}\");");
                sb.AppendLine("            }");
                sb.AppendLine($"            else");
                sb.AppendLine("            {");
                sb.AppendLine($"                sb.Append(\"{pair.Value[0].DbColumnName} BETWEEN @{pair.Value[0].Name} AND @{pair.Value[1].Name}\");");
                sb.AppendLine("            }");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
    }

    private static void GenerateAddParameters(StringBuilder sb, FilterModel filter)
    {
        sb.AppendLine("    public void AddParameters(global::Npgsql.NpgsqlParameterCollection parameters)");
        sb.AppendLine("    {");
        foreach (var pair in filter.PropertyGroups)
        {
            if (pair.Value.Length == 1)
            {
                sb.AppendLine($"        if ({pair.Value[0].Name} is not null) parameters.AddWithValue(\"{pair.Value[0].Name}\", {pair.Value[0].Name});");
            }
            else
            {
                sb.AppendLine($"        if ({pair.Value[0].Name} is not null)");
                sb.AppendLine("        {");
                sb.AppendLine($"            if ({pair.Value[0].Name} == {pair.Value[1].Name} || {pair.Value[1].Name} is null)");
                sb.AppendLine("            {");
                sb.AppendLine($"                if ({pair.Value[0].Name} is not null) parameters.AddWithValue(\"{pair.Key}\", {pair.Value[0].Name});");
                sb.AppendLine("            }");
                sb.AppendLine($"            else");
                sb.AppendLine("            {");
                sb.AppendLine($"                if ({pair.Value[0].Name} is not null) parameters.AddWithValue(\"{pair.Value[0].Name}\", {pair.Value[0].Name});");
                sb.AppendLine($"                if ({pair.Value[1].Name} is not null) parameters.AddWithValue(\"{pair.Value[1].Name}\", {pair.Value[1].Name});");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
        }

        sb.Remove(sb.Length - 1, 1);
        sb.AppendLine("    }");
    }

    private static void GenerateParse(StringBuilder sb, FilterModel filter)
    {
        string filterIFace = $"global::SzivarClubManager.Datasources.Database.Filters.IFilter<{filter.ModelType.GlobalName()}>";
        sb.AppendLine($"    public static {filterIFace} Parse(string filterString)");
        sb.AppendLine("    {");
        sb.AppendLine($"        {filter.FilterType.GlobalName()} filter = new();");
        sb.AppendLine();
        sb.AppendLine($"        global::System.Collections.Generic.IEnumerable<string> kvps = {FilterValueHandler}.SplitToEntries(filterString);");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine("            foreach (var split in global::System.Linq.Enumerable.Select(kvps, x => x.Split('=', 2)))");
        sb.AppendLine("            {");
        sb.AppendLine("                string key = split[0];");
        sb.AppendLine("                string value = split[1];");
        sb.AppendLine();
        sb.AppendLine("                switch (key)");
        sb.AppendLine("                {");
        foreach (var pair in filter.PropertyGroups)
        {
            sb.AppendLine($"                    case \"{pair.Key}\":");

            switch (pair.Value[0].Type)
            {
                case PropertyTypes.String:
                    sb.AppendLine($"                        filter.{pair.Value[0].Name} = {FilterValueHandler}.ParseStringValue(value);");
                    sb.AppendLine("                        break;");
                    break;
                case PropertyTypes.Int:
                    sb.AppendLine($"                        (filter.{pair.Value[0].Name}, filter.{pair.Value[1].Name}) = {FilterValueHandler}.ParseIntValues(value);");
                    sb.AppendLine("                        break;");
                    break;
                case PropertyTypes.Double:
                    sb.AppendLine($"                        (filter.{pair.Value[0].Name}, filter.{pair.Value[1].Name}) = {FilterValueHandler}.ParseDoubleValues(value);");
                    sb.AppendLine("                        break;");
                    break;
            }
        }

        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        catch");
        sb.AppendLine("        {");
        sb.AppendLine("             // ignored");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return filter;");
        sb.AppendLine("    }");
    }
}