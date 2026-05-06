using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.MobyGames.Models;
using MySqlConnector;

namespace Marechai.MobyGames.Services;

public class SourceDatabaseService
{
    readonly string _connectionString;

    public SourceDatabaseService(string connectionString) => _connectionString = connectionString;

    public async Task<List<string>> GetDistinctGameIdsAsync(int limit, int offset = 0)
    {
        var ids = new List<string>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT DISTINCT id FROM mobygames_raw ORDER BY id LIMIT @limit OFFSET @offset", connection);

        cmd.Parameters.AddWithValue("@limit",  limit);
        cmd.Parameters.AddWithValue("@offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();

        while(await reader.ReadAsync())
            ids.Add(reader.GetString(0));

        return ids;
    }

    public async Task<List<string>> GetUnprocessedGameIdsAsync(int batchSize, HashSet<string> processedIds)
    {
        var ids = new List<string>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT DISTINCT id FROM mobygames_raw", connection);

        await using var reader = await cmd.ExecuteReaderAsync();

        while(await reader.ReadAsync())
        {
            string id = reader.GetString(0);

            if(!processedIds.Contains(id))
                ids.Add(id);
        }

        ids.Sort(NaturalStringComparer.Instance);

        return ids.Count > batchSize ? ids.GetRange(0, batchSize) : ids;
    }

    public async Task<List<MobyGamesRawRow>> GetRowsForGameAsync(string gameId)
    {
        var rows = new List<MobyGamesRawRow>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT id, chunk, body FROM mobygames_raw WHERE id = @id ORDER BY chunk", connection);

        cmd.Parameters.AddWithValue("@id", gameId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while(await reader.ReadAsync())
        {
            rows.Add(new MobyGamesRawRow
            {
                Id    = reader.GetString(0),
                Chunk = reader.GetInt32(1),
                Body  = reader.GetString(2)
            });
        }

        return rows;
    }

    public async Task<int> GetTotalGameCountAsync()
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand("SELECT COUNT(DISTINCT id) FROM mobygames_raw", connection);
        var result = await cmd.ExecuteScalarAsync();

        return System.Convert.ToInt32(result);
    }

    public async Task InsertRowAsync(string gameId, int chunk, string body)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "INSERT INTO mobygames_raw (id, chunk, body) VALUES (@id, @chunk, @body)", connection);

        cmd.Parameters.AddWithValue("@id",    gameId);
        cmd.Parameters.AddWithValue("@chunk", chunk);
        cmd.Parameters.AddWithValue("@body",  body);

        await cmd.ExecuteNonQueryAsync();
    }
}
