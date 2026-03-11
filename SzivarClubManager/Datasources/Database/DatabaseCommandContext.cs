using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace SzivarClubManager.Datasources.Database;

public sealed class DatabaseCommandContext : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlCommand _command;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public NpgsqlParameterCollection Parameters => _command.Parameters;

    public DatabaseCommandContext(string query, NpgsqlConnection connection)
    {
        _command = new NpgsqlCommand(query, connection);
    }

    public async Task<NpgsqlDataReader> ExecuteReaderAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            return await _command.ExecuteReaderAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<int> ExecuteNonQueryAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            return await _command.ExecuteNonQueryAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<object?> ExecuteScalarAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            return await _command.ExecuteScalarAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public void Dispose()
    {
        _command.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
    }
}