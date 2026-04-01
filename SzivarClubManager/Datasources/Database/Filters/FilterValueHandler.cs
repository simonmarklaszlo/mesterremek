using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SzivarClubManager.Datasources.Database.Filters;

public static partial class FilterValueHandler
{
    public static string ToDisplayString(string value) => $"\"{value}\"";

    public static IEnumerable<string> SplitToEntries(string filterString)
    {
        if (!filterString.Contains('"')) return filterString.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var matches = KeyValuePairRegex().Matches(filterString);

        return matches.Select(x => x.Value);
    }

    public static string ParseStringValue(string value) => value.Replace("\"", "");

    public static (int?, int?) ParseIntValues(string value)
    {
        var match = IntValueRegex().Match(value);
        if (!match.Success) throw new FormatException("Invalid format");

        Group valGroup = match.Groups["val"];
        Group minGroup = match.Groups["min"];
        Group maxGroup = match.Groups["max"];

        if (valGroup.Success)
        {
            int val = int.Parse(valGroup.Value);
            return (val, val);
        }

        int? min = null;
        int? max = null;

        if (minGroup.Success) min = int.Parse(minGroup.Value);
        if (maxGroup.Success) max = int.Parse(maxGroup.Value);

        if (min is null && max is null) throw new FormatException("Invalid format");

        return (min, max);
    }

    public static (double?, double?) ParseDoubleValues(string value)
    {
        var match = DoubleValueRegex().Match(value);
        if (!match.Success) throw new FormatException("Invalid format");

        Group valGroup = match.Groups["val"];
        Group minGroup = match.Groups["min"];
        Group maxGroup = match.Groups["max"];

        if (valGroup.Success)
        {
            double val = double.Parse(valGroup.Value);
            return (val, val);
        }

        double? min = null;
        double? max = null;

        if (minGroup.Success) min = double.Parse(minGroup.Value);
        if (maxGroup.Success) max = double.Parse(maxGroup.Value);

        if (min is null && max is null) throw new FormatException("Invalid format");

        return (min, max);
    }


    /*
     \w+=(                                      <- key=
         -?\d+(?:\.\d+)?|                         <- value : 10, 10.5
                                                    OR
         \[-?\d+(?:\.\d+)?\.\.\-?d+(?:\.\d+)?\]     <- value : [10..20], [10.5..20], [10..20.5], [10.5..20.5]
                                                    OR
         "(?=.*\w)[\w\s]+"|                     <- value : "str", "str val", " str", "str "
                                                    OR
         \w+|                                   <- value : str
     )
     */
    [GeneratedRegex(@"\w+=(-?\d+(?:\.\d+)?|\[-?\d+(?:\.\d+)?\.\.-?\d+(?:\.\d+)?\]|""(?=.*\w)[\w\s]+""|\w+)")]
    private static partial Regex KeyValuePairRegex();

    [GeneratedRegex(@"(?<val>^-?\d+$)|(?:^\[(?<min>-?\d+)?\.\.(?<max>-?\d+)?\]$)")]
    private static partial Regex IntValueRegex();

    [GeneratedRegex(@"(?<val>^-?\d+(?:\.\d+)?$)|(?:^\[(?<min>-?\d+(?:\.\d+)?)?\.\.(?<max>-?\d+(?:\.\d+)?)?\]$)")]
    private static partial Regex DoubleValueRegex();
}