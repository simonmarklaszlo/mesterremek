using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(SuggestionType))]
public sealed class SuggsetionTypeFactory : IHelperFactory<SuggestionType>
{
    private readonly DatabaseConnection _connection;

    private SuggestionType[]? CachedItems
    {
        get;
        set
        {
            field = value;
            CacheUpdated = DateTime.Now;
        }
    } = null;

    public DateTime CacheUpdated { get; private set; }

    public SuggsetionTypeFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public async Task<SuggestionType[]> GetAll()
    {
        const string sql = "SELECT id, code, name, description, created_at FROM suggestion_types;";

        await using var command = _connection.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        List<SuggestionType> types = new(4);

        while (await reader.ReadAsync())
        {
            types.Add(new SuggestionType(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4)
            ));
        }

        return types.ToArray();
    }

    public async Task<SuggestionType[]> TryGetAllFromCache() => CachedItems ??= await GetAll();

    public async Task<SuggestionType[]> TryGetAllFromCache(int mustContainId)
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

    public Task<int> AddRange(IEnumerable<SuggestionType> items) => throw new NotSupportedException();
    public Task<int> EditRange(IEnumerable<SuggestionType> items) => throw new NotSupportedException();
    public Task<int> DeleteRange(IEnumerable<SuggestionType> items) => throw new NotSupportedException();
    public Task<SuggestionType?> GetModel(int id)
    {
        throw new NotImplementedException();
    }
    public Task<SuggestionType[]> GetModel(IEnumerable<int> ids)
    {
        throw new NotImplementedException();
    }
}