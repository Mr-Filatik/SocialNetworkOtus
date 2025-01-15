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
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            """
            CREATE TABLE IF NOT EXISTS messages (
                "id" BIGSERIAL PRIMARY KEY,
                "from" VARCHAR(255) NOT NULL,
                "to" VARCHAR(255) NOT NULL,
                "sending_time" TIMESTAMP NOT NULL,
                "text" TEXT NOT NULL);
            """, connection);
        using var reader = command.ExecuteReader();
        //DROP TABLE IF EXISTS messages;
    }

    public void Create(MessageEntity entity)
    {
        entity.SendingTime = DateTime.UtcNow;

        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO messages ("from", "to", "sending_time", "text")
            VALUES (@from, @to, @sending_time, @text);
            """, connection);
        command.Parameters.AddWithValue("from", entity.From);
        command.Parameters.AddWithValue("to", entity.To);
        command.Parameters.AddWithValue("sending_time", entity.SendingTime);
        command.Parameters.AddWithValue("text", entity.Text);
        using var reader = command.ExecuteReader();
    }

    public IEnumerable<MessageEntity> GetListLatest(string firstUser, string secondUser)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE ("from" = @first_user_id AND "to" = @second_user_id) OR ("from" = @second_user_id AND "to" = @first_user_id)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("first_user_id", firstUser);
        command.Parameters.AddWithValue("second_user_id", secondUser);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListNewest(string firstUser, string secondUser, long newest)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE (("from" = @first_user_id AND "to" = @second_user_id) OR ("from" = @second_user_id AND "to" = @first_user_id))
                AND ("id" > @newest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("first_user_id", firstUser);
        command.Parameters.AddWithValue("second_user_id", secondUser);
        command.Parameters.AddWithValue("newest", newest);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListOldest(string firstUser, string secondUser, long oldest)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE (("from" = @first_user_id AND "to" = @second_user_id) OR ("from" = @second_user_id AND "to" = @first_user_id))
                AND ("id" < @oldest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("first_user_id", firstUser);
        command.Parameters.AddWithValue("second_user_id", secondUser);
        command.Parameters.AddWithValue("oldest", oldest);
        command.Parameters.AddWithValue("limit", _limit);
        return GetList(command);
    }

    public IEnumerable<MessageEntity> GetListInRange(string firstUser, string secondUser, long newest, long oldest)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM messages
            WHERE (("from" = @first_user_id AND "to" = @second_user_id) OR ("from" = @second_user_id AND "to" = @first_user_id))
                AND ("id" < @newest AND "id" > @oldest)
            ORDER BY "id" DESC
            LIMIT @limit;
            """, connection);
        command.Parameters.AddWithValue("first_user_id", firstUser);
        command.Parameters.AddWithValue("second_user_id", secondUser);
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
                    Id = int.Parse(reader["id"].ToString()),
                    From = reader["from"].ToString(),
                    To = reader["to"].ToString(),
                    SendingTime = DateTime.Parse(reader["sending_time"].ToString()),
                    Text = reader["text"].ToString(),
                });
            }
        }
        return entities;
    }

    #endregion
}
