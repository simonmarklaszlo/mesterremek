using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(SuggestionStatus))]
public sealed class SuggestionStatusFactory : IHelperFactory<SuggestionStatus>
{
    private readonly DatabaseConnection _connection;

    private SuggestionStatus[]? CachedItems
    {
        get;
        set
        {
            field = value;
            CacheUpdated = DateTime.Now;
        }
    } = null;

    public DateTime CacheUpdated { get; private set; }

    public SuggestionStatusFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public async Task<SuggestionStatus[]> GetAll()
    {
        const string query = "SELECT id, code, name, description, created_at FROM suggestion_statuses;";

        await using var command = _connection.CreateCommand(query);
        await using var reader = await command.ExecuteReaderAsync();

        List<SuggestionStatus> statuses = new(4);

        while (await reader.ReadAsync())
        {
            statuses.Add(new SuggestionStatus(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4)
            ));
        }

        return statuses.ToArray();
    }

    public async Task<SuggestionStatus[]> TryGetAllFromCache() => CachedItems ??= await GetAll();

    public async Task<SuggestionStatus[]> TryGetAllFromCache(int mustContainId)
    {
        if (CachedItems is null)
        {
            return CachedItems = await GetAll();
        }

        if (CachedItems.Any(x => x.Id == mustContainId))
        {
            return CachedItems;
        }

        return CachedItems = await GetAll();
    }

    public void InvalidateCache() => CachedItems = null;

    public Task<int> AddRange(IEnumerable<SuggestionStatus> items) => throw new NotSupportedException();
    public Task<int> EditRange(IEnumerable<SuggestionStatus> items) => throw new NotSupportedException();
    public Task<int> DeleteRange(IEnumerable<SuggestionStatus> items) => throw new NotSupportedException();
    public Task<SuggestionStatus?> GetModel(int id)
    {
        throw new NotImplementedException();
    }
    public Task<SuggestionStatus[]> GetModel(IEnumerable<int> ids)
    {
        throw new NotImplementedException();
    }
}