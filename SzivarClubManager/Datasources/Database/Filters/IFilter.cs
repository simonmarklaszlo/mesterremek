using System;
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
    static IFilter<T> Empty => throw new NotSupportedException();
    static virtual IFilter<T> Parse(string filterString) => throw new NotSupportedException();
}