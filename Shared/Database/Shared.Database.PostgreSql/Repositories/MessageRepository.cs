using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

public class MessageRepository
{
    private readonly int _limit = 20;

    private readonly PostgreDatabaseSelector _databaseSelector;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(PostgreDatabaseSelector databaseSelector, ILogger<MessageRepository> logger)
    {
        _databaseSelector = databaseSelector;
        _logger = logger;
    }

    public void Init()
    {
        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            """
            CREATE TABLE IF NOT EXISTS messages (
                "id" BIGSERIAL NOT NULL,
                "from" VARCHAR(255) NOT NULL,
                "to" VARCHAR(255) NOT NULL,
                "from_to_hash" BIGSERIAL NOT NULL,
                "sending_time" TIMESTAMP NOT NULL,
                "text" TEXT NOT NULL,
                PRIMARY KEY ("id", "from_to_hash"));
            """, connection);
        using var reader = command.ExecuteReader();
        //DROP TABLE IF EXISTS messages;
    }

    public void Create(MessageEntity entity)
    {
        entity.SendingTime = DateTime.UtcNow;
        entity.FromToHash = GetDeterministicHashCode(entity.From, entity.To);

        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO messages ("from", "to", "from_to_hash", "sending_time", "text")
            VALUES (@from, @to, @from_to_hash, @sending_time, @text);
            """, connection);
        command.Parameters.AddWithValue("from", entity.From);
        command.Parameters.AddWithValue("to", entity.To);
        command.Parameters.AddWithValue("from_to_hash", entity.FromToHash);
        command.Parameters.AddWithValue("sending_time", entity.SendingTime);
        command.Parameters.AddWithValue("text", entity.Text);
        using var reader = command.ExecuteReader();
    }

    public IEnumerable<MessageEntity> GetListLatest(string firstUser, string secondUser)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE "from_to_hash" = @from_to_hash
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("from_to_hash", fromToHash);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListNewest(string firstUser, string secondUser, long newest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE ("from_to_hash" = @from_to_hash) AND ("id" > @newest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("from_to_hash", fromToHash);
        command.Parameters.AddWithValue("newest", newest);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListOldest(string firstUser, string secondUser, long oldest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE ("from_to_hash" = @from_to_hash) AND ("id" < @oldest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("from_to_hash", fromToHash);
        command.Parameters.AddWithValue("oldest", oldest);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListInRange(string firstUser, string secondUser, long newest, long oldest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        using var connection = _databaseSelector.GetDatabase(withSharding: true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE ("from_to_hash" = @from_to_hash) AND ("id" < @newest AND "id" > @oldest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("from_to_hash", fromToHash);
        command.Parameters.AddWithValue("newest", newest);
        command.Parameters.AddWithValue("oldest", oldest);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    #region Private Methods

    private IEnumerable<MessageEntity> GetList(NpgsqlCommand? command)
    {
        using var reader = command.ExecuteReader();
        var entities = new List<MessageEntity>();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                entities.Add(new MessageEntity()
                {
                    Id = long.Parse(reader["id"].ToString()),
                    From = reader["from"].ToString(),
                    To = reader["to"].ToString(),
                    FromToHash = long.Parse(reader["from_to_hash"].ToString()),
                    SendingTime = DateTime.Parse(reader["sending_time"].ToString()),
                    Text = reader["text"].ToString(),
                });
            }
        }
        return entities;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strs"></param>
    /// <returns></returns>
    /// <remarks> <see href="https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/"/> </remarks>
    private long GetDeterministicHashCode(params string[] strs)
    {
        var sortStrs = strs.Order();
        unchecked
        {
            int hash1 = (5381 << 16) + 5381; //change to long
            int hash2 = hash1; //change to long

            for (int i = 0; i < sortStrs.First().Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ sortStrs.First()[i];
                if (i == sortStrs.First().Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ sortStrs.First()[i + 1];
            }

            for (int i = 0; i < sortStrs.Last().Length; i += 2)
            {
                hash1 = ((hash1 << 10) + hash1) ^ sortStrs.Last()[i];
                if (i == sortStrs.Last().Length - 1)
                    break;
                hash2 = ((hash2 << 10) + hash2) ^ sortStrs.Last()[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    #endregion
}
