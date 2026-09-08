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
            "INSERT IGNORE INTO mobygames_raw (id, chunk, body) VALUES (@id, @chunk, @body)", connection);

        cmd.Parameters.AddWithValue("@id",    gameId);
        cmd.Parameters.AddWithValue("@chunk", chunk);
        cmd.Parameters.AddWithValue("@body",  body);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Moves a row from one chunk number to another. Used by the chunk migrator
    ///     to reassign dynamically-allocated chunks to their fixed slot numbers.
    ///     Returns the number of affected rows (0 if the source chunk didn't exist,
    ///     or the target chunk already exists via <c>INSERT IGNORE</c>).
    /// </summary>
    public async Task<int> MoveChunkAsync(string gameId, int fromChunk, int toChunk)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Copy to new chunk (IGNORE if target already exists), then delete old
        await using var insert = new MySqlCommand(
            "INSERT IGNORE INTO mobygames_raw (id, chunk, body) SELECT id, @toChunk, body FROM mobygames_raw WHERE id = @id AND chunk = @fromChunk",
            connection);

        insert.Parameters.AddWithValue("@id",        gameId);
        insert.Parameters.AddWithValue("@fromChunk", fromChunk);
        insert.Parameters.AddWithValue("@toChunk",   toChunk);

        int inserted = await insert.ExecuteNonQueryAsync();

        await using var delete = new MySqlCommand(
            "DELETE FROM mobygames_raw WHERE id = @id AND chunk = @fromChunk", connection);

        delete.Parameters.AddWithValue("@id",        gameId);
        delete.Parameters.AddWithValue("@fromChunk", fromChunk);

        await delete.ExecuteNonQueryAsync();

        return inserted;
    }

    /// <summary>
    ///     Returns all (id, chunk) pairs whose chunk number is outside the known fixed slots
    ///     (0-5 and 10-12). These are candidates for chunk migration.
    /// </summary>
    public async Task<List<(string Id, int Chunk)>> GetMisplacedChunksAsync()
    {
        var results = new List<(string, int)>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT id, chunk FROM mobygames_raw WHERE chunk NOT IN (0,1,2,3,4,5,10,11,12) ORDER BY id, chunk",
            connection);

        await using var reader = await cmd.ExecuteReaderAsync();

        while(await reader.ReadAsync())
            results.Add((reader.GetString(0), reader.GetInt32(1)));

        return results;
    }

    /// <summary>
    ///     Returns the body of a single (id, chunk) row. Used by the chunk migrator
    ///     to detect the page type without loading all chunks for a game.
    /// </summary>
    public async Task<string> GetChunkBodyAsync(string gameId, int chunk)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT body FROM mobygames_raw WHERE id = @id AND chunk = @chunk LIMIT 1", connection);

        cmd.Parameters.AddWithValue("@id",    gameId);
        cmd.Parameters.AddWithValue("@chunk", chunk);

        var result = await cmd.ExecuteScalarAsync();

        return result as string;
    }

    /// <summary>
    ///     Returns true if a row exists for the given (id, chunk) pair.
    /// </summary>
    public async Task<bool> ChunkExistsAsync(string gameId, int chunk)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT EXISTS (SELECT 1 FROM mobygames_raw WHERE id = @id AND chunk = @chunk LIMIT 1)", connection);

        cmd.Parameters.AddWithValue("@id",    gameId);
        cmd.Parameters.AddWithValue("@chunk", chunk);

        var result = await cmd.ExecuteScalarAsync();

        return System.Convert.ToInt32(result) == 1;
    }

    /// <summary>
    ///     Delete every cached chunk for the given slug. Used to evict stale legacy-layout
    ///     captures before re-scraping the current new-layout page (see
    ///     <c>DlcRelationService.ResolveBaseSoftwareIdAsync</c>) — MobyGames data corrections
    ///     since the 2019 capture mean the cached HTML often disagrees with the live page
    ///     (e.g. mis-tagged genres) and would re-poison every downstream import otherwise.
    /// </summary>
    /// <summary>Chunk numbers cached for the slug (no bodies), in ascending order.</summary>
    public async Task<List<int>> GetChunkNumbersAsync(string gameId)
    {
        var chunks = new List<int>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT chunk FROM mobygames_raw WHERE id = @id ORDER BY chunk", connection);

        cmd.Parameters.AddWithValue("@id", gameId);

        await using var reader = await cmd.ExecuteReaderAsync();

        while(await reader.ReadAsync())
            chunks.Add(reader.GetInt32(0));

        return chunks;
    }

    public async Task<int> DeleteAllChunksAsync(string gameId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "DELETE FROM mobygames_raw WHERE id = @id", connection);

        cmd.Parameters.AddWithValue("@id", gameId);

        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Returns <c>true</c> if at least one <c>mobygames_raw</c> row exists for the supplied slug.
    ///     Short-circuited via <c>EXISTS</c> + <c>LIMIT 1</c> so the cost is independent of the number
    ///     of chunks already stored for that game.
    /// </summary>
    public async Task<bool> GameExistsAsync(string gameId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new MySqlCommand(
            "SELECT EXISTS (SELECT 1 FROM mobygames_raw WHERE id = @id LIMIT 1)", connection);

        cmd.Parameters.AddWithValue("@id", gameId);

        var result = await cmd.ExecuteScalarAsync();

        return System.Convert.ToInt32(result) == 1;
    }
}
