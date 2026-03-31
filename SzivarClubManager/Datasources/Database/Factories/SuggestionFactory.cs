using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Database.Filters;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.SourceGeneration.Factory;

namespace SzivarClubManager.Datasources.Database.Factories;

[Factory]
public sealed class SuggestionFactory : IPageFactory<Suggestion>
{
    private const string TableName = "suggestions";
    private readonly DatabaseConnection _connection;
    private readonly SuggestionStatusFactory _suggestionStatusFactory;
    private readonly SuggsetionTypeFactory _suggestionTypeFactory;
    private readonly UserFactory _userFactory;
    private readonly ShopFactory _shopFactory;
    private readonly SuggestionVotesFactory _suggestionVotesFactory;

    public SuggestionFactory(DatabaseConnection connection, SuggestionStatusFactory suggestionStatusFactory, SuggsetionTypeFactory suggestionTypeFactory, UserFactory userFactory,
        ShopFactory shopFactory, SuggestionVotesFactory suggestionVotesFactory)
    {
        _connection = connection;
        _suggestionStatusFactory = suggestionStatusFactory;
        _suggestionTypeFactory = suggestionTypeFactory;
        _userFactory = userFactory;
        _shopFactory = shopFactory;
        _suggestionVotesFactory = suggestionVotesFactory;
    }

    public Task<bool> PageExists(int page, int pageSize, IFilter<Suggestion> filter) => CommonQueries.PageExitstOnTable(_connection, TableName, page, pageSize, filter);
    public Task<int> GetLastPage(int pageSize, IFilter<Suggestion> filter) => CommonQueries.GetLastPageOnTable(_connection, TableName, pageSize, filter);


    public async Task<Suggestion[]> GetPage(int page, int pageSize, IFilter<Suggestion> filter)
    {
        SuggestionStatus[] statuses = await _suggestionStatusFactory.TryGetAllFromCache();
        SuggestionType[] types = await _suggestionTypeFactory.TryGetAllFromCache();

        const string query = """
                             SELECT id, type_id, shop_id, proposed_value, additional_data, user_id, status_id, created_at, updated_at
                             FROM suggestions
                             LIMIT @limit OFFSET @offset
                             """;

        List<(int id, int typeId, int? shopId, string proposedValue, string? additionalData, int userId, int statusId, DateTime createdAt, DateTime updatedAt)> suggestions = new(pageSize);
        await using (var command = _connection.CreateCommand(query))
        {
            command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
            command.Parameters.AddWithValue("limit", pageSize);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suggestions.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetNullableInt32(2),
                    reader.GetString(3),
                    reader.GetNullableString(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetDateTime(7),
                    reader.GetDateTime(8)
                ));
            }
        }

        Shop[] shops = await _shopFactory.GetModel(suggestions.Select(x => x.shopId).Where(x => x.HasValue).Select(x => x!.Value));
        User[] users = await _userFactory.GetModel(suggestions.Select(x => x.userId));
        SuggestionVotes[] votes = await _suggestionVotesFactory.GetVotes(suggestions.Select(x => x.id));


        return suggestions.Select(x => Suggestion.Create(
            x.id,
            types.First(y => y.Id == x.typeId),
            statuses.First(y => y.Id == x.statusId),
            users.First(y => y.Id == x.userId),
            x.createdAt,
            x.updatedAt,
            votes.FirstOrDefault(y => y.SuggestionId == x.id) ?? new SuggestionVotes(x.id, 0, 0, DateTime.Now, DateTime.Now),
            shops.FirstOrDefault(y => y.Id == x.shopId),
            x.proposedValue,
            x.additionalData
        )).ToArray();
    }

    public async Task<Suggestion[]> GetPageOnFlagged(int page, int pageSize, IFilter<Suggestion> filter)
    {
        SuggestionStatus[] statuses = await _suggestionStatusFactory.TryGetAllFromCache();
        SuggestionType[] types = await _suggestionTypeFactory.TryGetAllFromCache();

        const string query = """
                             SELECT id, type_id, shop_id, proposed_value, additional_data, user_id, status_id, created_at, updated_at
                             FROM suggestions
                             WHERE id IN (SELECT suggestion_id FROM public.flagged_suggestions)
                             LIMIT @limit OFFSET @offset
                             """;

        List<(int id, int typeId, int? shopId, string proposedValue, string? additionalData, int userId, int statusId, DateTime createdAt, DateTime updatedAt)> suggestions = new(pageSize);
        await using (var command = _connection.CreateCommand(query))
        {
            command.Parameters.AddWithValue("offset", (page - 1) * pageSize);
            command.Parameters.AddWithValue("limit", pageSize);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suggestions.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetNullableInt32(2),
                    reader.GetString(3),
                    reader.GetNullableString(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetDateTime(7),
                    reader.GetDateTime(8)
                ));
            }
        }

        Shop[] shops = await _shopFactory.GetModel(suggestions.Select(x => x.shopId).Where(x => x.HasValue).Select(x => x!.Value));
        User[] users = await _userFactory.GetModel(suggestions.Select(x => x.userId));
        SuggestionVotes[] votes = await _suggestionVotesFactory.GetVotes(suggestions.Select(x => x.id));


        return suggestions.Select(x => Suggestion.Create(
            x.id,
            types.First(y => y.Id == x.typeId),
            statuses.First(y => y.Id == x.statusId),
            users.First(y => y.Id == x.userId),
            x.createdAt,
            x.updatedAt,
            votes.FirstOrDefault(y => y.SuggestionId == x.id) ?? new SuggestionVotes(x.id, 0, 0, DateTime.Now, DateTime.Now),
            shops.FirstOrDefault(y => y.Id == x.shopId),
            x.proposedValue,
            x.additionalData
        )).ToArray();
    }

    public Task<int> AddRange(IEnumerable<Suggestion> items) => throw new NotSupportedException();
    public Task<int> EditRange(IEnumerable<Suggestion> items) => throw new NotSupportedException();
    public Task<int> DeleteRange(IEnumerable<Suggestion> items) => throw new NotSupportedException();

    public async Task<Suggestion?> GetModel(int id)
    {
        SuggestionStatus[] statuses = await _suggestionStatusFactory.TryGetAllFromCache();
        SuggestionType[] types = await _suggestionTypeFactory.TryGetAllFromCache();

        const string query = """
                             SELECT id, type_id, shop_id, proposed_value, additional_data, user_id, status_id, created_at, updated_at
                             FROM suggestions
                             WHERE id = @id
                             LIMIT 1
                             """;

        (int id, int typeId, int? shopId, string proposedValue, string? additionalData, int userId, int statusId, DateTime createdAt, DateTime updatedAt) suggestion;
        await using (var command = _connection.CreateCommand(query))
        {
            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            suggestion = (
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetNullableInt32(2),
                reader.GetString(3),
                reader.GetNullableString(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetDateTime(7),
                reader.GetDateTime(8)
            );
        }

        Shop? shop = suggestion.shopId.HasValue ? await _shopFactory.GetModel(suggestion.shopId.Value) : null;
        User user = (await _userFactory.GetModel(suggestion.userId))!;
        SuggestionVotes? vote = await _suggestionVotesFactory.GetVotes(suggestion.id);

        return Suggestion.Create(
            suggestion.id,
            types.First(y => y.Id == suggestion.typeId),
            statuses.First(y => y.Id == suggestion.statusId),
            user,
            suggestion.createdAt,
            suggestion.updatedAt,
            vote ?? new SuggestionVotes(suggestion.id, 0, 0, DateTime.Now, DateTime.Now),
            shop,
            suggestion.proposedValue,
            suggestion.additionalData
        );
    }

    public async Task ApproveSuggestion(Suggestion suggestion)
    {
        throw new NotImplementedException();
    }

    public async Task DenySuggestion(Suggestion suggestion)
    {
        throw new NotImplementedException();
    }

    public Task<Suggestion[]> GetModel(IEnumerable<int> ids)
    {
        throw new NotImplementedException();
    }
}