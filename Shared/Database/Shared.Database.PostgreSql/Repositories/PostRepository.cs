using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

public class PostRepository
{
    private readonly PostgreDatabaseSelector _databaseSelector;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(PostgreDatabaseSelector databaseSelector, ILogger<PostRepository> logger)
    {
        _databaseSelector = databaseSelector;
        _logger = logger;
    }

    public PostEntity? Create(string userId, string content)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO public.posts
            (author_id, content)
            VALUES(@author_id, @content) RETURNING *;
            """, connection);
        command.Parameters.AddWithValue("author_id", userId);
        command.Parameters.AddWithValue("content", content);
        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            return new PostEntity()
            {
                PostId = int.Parse(reader["post_id"].ToString()),
                AuthorId = reader["author_id"].ToString(),
                Content = reader["content"].ToString(),
            };
        }
        return null;
    }

    public int Update(string userId, int postId, string newContent)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            UPDATE public.posts
            SET content = @content
            WHERE post_id = @post_id AND author_id = @author_id;
            """, connection);
        command.Parameters.AddWithValue("author_id", userId);
        command.Parameters.AddWithValue("post_id", postId);
        command.Parameters.AddWithValue("content", newContent);
        using var reader = command.ExecuteReader();

        return 1;
    }

    public int Delete(string userId, int postId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            DELETE FROM public.posts
            WHERE author_id = @author_id AND post_id = @post_id;
            """, connection);
        command.Parameters.AddWithValue("author_id", userId);
        command.Parameters.AddWithValue("post_id", postId);
        using var reader = command.ExecuteReader();

        return 1;
    }
}
