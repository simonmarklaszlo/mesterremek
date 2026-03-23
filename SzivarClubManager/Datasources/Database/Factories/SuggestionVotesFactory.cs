using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SzivarClubManager.Datasources.Factory;
using SzivarClubManager.Models.Suggestions;
using SzivarClubManager.SourceGeneration;

namespace SzivarClubManager.Datasources.Database.Factories;

[FactoryOf(typeof(SuggestionVotes))]
public class SuggestionVotesFactory : IFactory
{
    private readonly DatabaseConnection _connection;

    public SuggestionVotesFactory(DatabaseConnection connection)
    {
        _connection = connection;
    }

    public async Task<SuggestionVotes?> GetVotes(int suggestionId)
    {
        const string query = """
                             SELECT 
                                 suggestion_id,
                                 COUNT(*) FILTER (WHERE vote_type = 'like') AS like_count,
                                 COUNT(*) FILTER (WHERE vote_type = 'dislike') AS dislike_count,
                                 MIN(created_at) AS created_at,
                                 MAX(updated_at) AS updated_at
                             FROM suggestion_votes
                             WHERE suggestion_id = @SuggestionId
                             GROUP BY suggestion_id;
                             """;

        await using var command = _connection.CreateCommand(query);
        command.Parameters.AddWithValue("SuggestionId", suggestionId);
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new SuggestionVotes(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetDateTime(3),
                reader.GetDateTime(4)
            );
        }

        return null;
    }

    public async Task<SuggestionVotes[]> GetVotes(IEnumerable<int> suggestionIds)
    {
        int[] suggestionIdsArr = suggestionIds.ToArray();
        if (suggestionIdsArr.Length == 0) return [];

        const string query = """
                             SELECT 
                                 suggestion_id,
                                 COUNT(*) FILTER (WHERE vote_type = 'like') AS like_count,
                                 COUNT(*) FILTER (WHERE vote_type = 'dislike') AS dislike_count,
                                 MIN(created_at) AS created_at,
                                 MAX(updated_at) AS updated_at
                             FROM suggestion_votes
                             WHERE suggestion_id = ANY(@suggestionIds)
                             GROUP BY suggestion_id;
                             """;

        await using var command = _connection.CreateCommand(query);
        command.Parameters.AddWithValue("SuggestionIds", suggestionIdsArr);
        await using var reader = await command.ExecuteReaderAsync();

        List<SuggestionVotes> votes = new(suggestionIdsArr.Length);

        while (await reader.ReadAsync())
        {
            votes.Add(new SuggestionVotes(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetDateTime(3),
                reader.GetDateTime(4)
            ));
        }

        return votes.ToArray();
    }
}