using System;

namespace SzivarClubManager.ViewModels.Activities.Page.Filter;

public enum FieldState
{
    None,
    Invalid,
    Warning,
    Valid,
}

public static class FieldStateExtensions
{
    extension(FieldState state)
    {
        public static FieldState CheckOptionalStringField(string value, Func<string, bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(value)) return FieldState.None;

            if (!predicate(value)) return FieldState.Invalid;

            return FieldState.Valid;
        }

        public static FieldState CheckOptionalIntField(string value, Func<int, bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(value)) return FieldState.None;

            if (!int.TryParse(value, out int number)) return FieldState.Invalid;

            if (predicate(number)) return FieldState.Valid;

            return FieldState.Warning;
        }

        public static FieldState CheckOptionalDoubleField(string value, Func<double, bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(value)) return FieldState.None;

            if (!double.TryParse(value, out double number)) return FieldState.Invalid;

            if (predicate(number)) return FieldState.Valid;

            return FieldState.Warning;
        }
    }
}