using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Npgsql;
using SzivarClubManager.Models;

namespace SzivarClubManager.Datasources.Database.Filters;

public partial interface IFilter<T> where T : class, IModel
{
    bool IsEmpty { get; }

    void CopyTo(IFilter<T> other);

    string ToString();

    /// <summary>
    /// Only where clause
    /// </summary>
    string ConstructParameterizedQuery();

    void AddParameters(NpgsqlParameterCollection parameters);

    protected static string ToDisplayString(string value) => $"\"{value}\"";

    protected static IEnumerable<string> SplitToEntries(string filterString)
    {
        if (!filterString.Contains('"')) return filterString.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var matches = StringValueRegex().Matches(filterString);

        return matches
            .Select(x => x.Value);
    }

    protected static string ParseStringValue(string value) => value.Replace("\"", "");

    protected static (int, int) ParseIntValues(string value)
    {
        if (value[0] != '[' && int.TryParse(value, out int id))
        {
            return (id, id);
        }

        {
            var range = value.Trim('[', ']').Split("..", StringSplitOptions.RemoveEmptyEntries);
            if (range.Length == 2 && int.TryParse(range[0], out int minId) && int.TryParse(range[1], out int maxId))
            {
                return (minId, maxId);
            }
        }

        throw new InvalidOperationException("Invalid format");
    }

    protected static (double, double) ParseDoubleValues(string value)
    {
        if (value[0] != '[' && double.TryParse(value, out double singleVal))
        {
            return (singleVal, singleVal);
        }

        var range = value.Trim('[', ']').Split("..", StringSplitOptions.RemoveEmptyEntries);
        if (range.Length == 2 && int.TryParse(range[0], out int minVal) && int.TryParse(range[1], out int maxVal))
        {
            return (minVal, maxVal);
        }


        throw new InvalidOperationException("Invalid format");
    }

    static virtual IFilter<T> Parse(string filterString) => throw new NotSupportedException();

    [GeneratedRegex("""\w+=("[^"]*"|\S+)""")]
    private static partial Regex StringValueRegex();
}