using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.Entities;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

public class UserRepository
{
    private readonly PostgreDatabaseSelector _databaseSelector;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(PostgreDatabaseSelector databaseSelector, ILogger<UserRepository> logger)
    {
        _databaseSelector = databaseSelector;
        _logger = logger;
    }

    public void Init()
    {
        //delete later
        try
        {
            using var connection = _databaseSelector.GetDatabase().OpenConnection();
            using var command = new NpgsqlCommand(
                """
                    CREATE TABLE IF NOT EXISTS users (
                        user_id text primary key,
                        first_name text NOT NULL,
                        second_name text NOT NULL,
                        password_hash text NOT NULL,
                        password_salt text NOT NULL,
                        gender boolean NOT NULL,
                        birth_date timestamp NOT NULL,
                        city text NOT NULL,
                        interests text[] NOT NULL);
                    """, connection);
            using var reader = command.ExecuteReader();
            // DROP TABLE IF EXISTS users;

            // Connections must be different for each request, because there is no support multiple commands.
            using var connectionFriends = _databaseSelector.GetDatabase().OpenConnection();
            using var commandFriends = new NpgsqlCommand(
                """
                    CREATE TABLE IF NOT EXISTS friends (
                        user_id text,
                        friend_id text,
                        PRIMARY KEY(user_id, friend_id));
                    """, connectionFriends);
            using var readerFriends = commandFriends.ExecuteReader();

            // postgre numeric datatypes https://www.postgresql.org/docs/current/datatype-numeric.html
            using var connectionPosts = _databaseSelector.GetDatabase().OpenConnection();
            using var commandPosts = new NpgsqlCommand(
                """
                    CREATE TABLE IF NOT EXISTS posts (
                        post_id serial,
                        author_id text NULL,
                        content text,
                        PRIMARY KEY(post_id));
                    """, connectionPosts);
            using var readerPosts = commandPosts.ExecuteReader();

            //https://www.postgresql.org/docs/current/sql-insert.html

            Add(new UserEntity()
            {
                Id = "e0c8b889-d677-4e10-9d9b-cd202a113bda",
                FirstName = "Vladislav",
                SecondName = "Filatov",
                PasswordHash = "password",
                Gender = Entities.Types.Gender.Male,
                DateOfBirth = new DateTime(2000, 1, 10),
                City = "Saratov",
                Interests = ["Sport", "Programming", "Psychology"],
            });

            Add(new UserEntity()
            {
                Id = "00fc8afc-6b99-43b2-962c-7a806b0816fe",
                FirstName = "Vladislava",
                SecondName = "Filatova",
                PasswordHash = "password",
                Gender = Entities.Types.Gender.Female,
                DateOfBirth = new DateTime(2000, 2, 20),
                City = "Moscow",
                Interests = ["Sport", "TV series", "Movies"]
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error {e.Message}");
        }
    }

    public string Add(UserEntity user)
    {

        if (string.IsNullOrEmpty(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        var passData = HashPassword(user.PasswordHash);
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO users (user_id ,first_name, second_name, password_hash, password_salt, gender, birth_date, city, interests)
            VALUES (@user_id, @first_name, @second_name, @password_hash, @password_salt, @gender, @birth_date, @city, @interests)
            ON CONFLICT (user_id) DO NOTHING;
            """, connection);
        command.Parameters.AddWithValue("user_id", user.Id);
        command.Parameters.AddWithValue("first_name", user.FirstName);
        command.Parameters.AddWithValue("second_name", user.SecondName);
        command.Parameters.AddWithValue("password_hash", passData.passwordHash);
        command.Parameters.AddWithValue("password_salt", passData.passwordSalt);
        command.Parameters.AddWithValue("gender", (user.Gender == Entities.Types.Gender.Male));
        command.Parameters.AddWithValue("birth_date", user.DateOfBirth.Date);
        command.Parameters.AddWithValue("city", user.City);
        command.Parameters.AddWithValue("interests", user.Interests);
        using var reader = command.ExecuteReader();

        return user.Id;
    }

    public string AddFriend(string userId, string friendId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO friends (user_id, friend_id)
            VALUES (@user_id, @friend_id)
            ON CONFLICT (user_id, friend_id) DO NOTHING;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("friend_id", friendId);
        using var reader = command.ExecuteReader();

        return $"{userId}:{friendId}";
    }

    public string DeleteFriend(string userId, string friendId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            DELETE FROM friends
            WHERE user_id = @user_id AND friend_id = @friend_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("friend_id", friendId);
        using var reader = command.ExecuteReader();

        return $"{userId}:{friendId}";
    }

    public IEnumerable<string> GetFriendIds(string userId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT user_id
            FROM public.friends
            WHERE friend_id = @friend_id;
            """, connection);
        command.Parameters.AddWithValue("friend_id", userId);
        using var reader = command.ExecuteReader();

        var posts = new List<string>();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                posts.Add(reader["user_id"].ToString());
            }
        }
        return posts;
    }

    public IEnumerable<PostEntity> GetPosts(string userId, int limit, int offset = 0)
    {
        if (limit < 1)
        {
            limit = 1;
        }
        if (offset < 0)
        {
            offset = 0;
        }
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM public.posts
            WHERE author_id IN (
            	SELECT friend_id
            	FROM public.friends
            	WHERE user_id = @user_id)
            ORDER BY post_id DESC
            LIMIT @limit OFFSET @offset;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("limit", limit);
        command.Parameters.AddWithValue("offset", offset);
        using var reader = command.ExecuteReader();
        var posts = new List<PostEntity>();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                posts.Add(new PostEntity()
                {
                    PostId = int.Parse(reader["post_id"].ToString()),
                    AuthorId = reader["author_id"].ToString(),
                    Content = reader["content"].ToString(),
                });
            }
        }
        return posts;
    }

    public int GetPostLag(string userId, int lastPostId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT COUNT(*) as count
            FROM public.posts
            WHERE author_id IN (
            	SELECT friend_id
            	FROM public.friends
            	WHERE user_id = @user_id) AND post_id > @post_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("post_id", lastPostId);
        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            return int.Parse(reader["count"].ToString());
        }
        return 0;
    }

    public IEnumerable<PostEntity> GetMyPosts(string userId, int limit, int offset = 0)
    {
        if (limit < 1)
        {
            limit = 1;
        }
        if (offset < 0)
        {
            offset = 0;
        }
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM public.posts
            WHERE author_id  = @user_id
            ORDER BY post_id DESC
            LIMIT @limit OFFSET @offset;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("limit", limit);
        command.Parameters.AddWithValue("offset", offset);
        using var reader = command.ExecuteReader();
        var posts = new List<PostEntity>();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                posts.Add(new PostEntity()
                {
                    PostId = int.Parse(reader["post_id"].ToString()),
                    AuthorId = reader["author_id"].ToString(),
                    Content = reader["content"].ToString(),
                });
            }
        }
        return posts;
    }

    public PostEntity? GetPost(string userId, int postId)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM public.posts
            WHERE (author_id IN (
            	SELECT friend_id
            	FROM public.friends
            	WHERE user_id = @user_id) OR author_id = @user_id)
            AND post_id = @post_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("post_id", postId);
        using var reader = command.ExecuteReader();
        var posts = new List<PostEntity>();
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

    public PostEntity? CreatePost(string userId, string content)
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

    public int DeletePost(string userId, int postId)
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

    public int UpdatePost(string userId, int postId, string newContent)
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

    public bool VerifyPassword(string id, string password)
    {
        var user = Get(id);

        var hashToCompare = HashPassword(password, Convert.FromHexString(user.PasswordSalt));

        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(hashToCompare.passwordHash), Convert.FromHexString(user.PasswordHash)); ;
    }

    public UserEntity? Get(string id)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM users
            WHERE user_id=@user_id;
            """, connection);
        command.Parameters.AddWithValue("user_id", id);
        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            return new UserEntity()
            {
                Id = reader["user_id"].ToString(),
                FirstName = reader["first_name"].ToString(),
                SecondName = reader["second_name"].ToString(),
                Gender = bool.Parse(reader["gender"].ToString()) ? Entities.Types.Gender.Male : Entities.Types.Gender.Female,
                DateOfBirth = DateTime.Parse(reader["birth_date"].ToString()),
                PasswordHash = reader["password_hash"].ToString(), //dont return ?
                PasswordSalt = reader["password_salt"].ToString(), //dont return ?
                City = reader["city"].ToString(),
                Interests = reader["interests"] as string[],
            };
        }
        return null;
    }

    public IEnumerable<UserEntity> Search(string firstNamePart, string secondNamePart)
    {
        using var connection = _databaseSelector.GetDatabase(true).OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM users
            WHERE LOWER(first_name) LIKE '%' || LOWER(@first_name) || '%' AND LOWER(second_name) LIKE '%' || LOWER(@second_name) || '%';
            """, connection);
        command.Parameters.AddWithValue("first_name", firstNamePart);
        command.Parameters.AddWithValue("second_name", secondNamePart);
        using var reader = command.ExecuteReader();
        var users = new List<UserEntity>();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                users.Add(new UserEntity()
                {
                    Id = reader["id"].ToString(),
                    FirstName = reader["first_name"].ToString(),
                    SecondName = reader["second_name"].ToString(),
                    Gender = bool.Parse(reader["gender"].ToString()) ? Entities.Types.Gender.Male : Entities.Types.Gender.Female,
                    DateOfBirth = DateTime.Parse(reader["birth_date"].ToString()),
                    PasswordHash = reader["password_hash"].ToString(), //dont return ?
                    PasswordSalt = reader["password_salt"].ToString(), //dont return ?
                    City = reader["city"].ToString(),
                    Interests = reader["interests"] as string[],
                });
            }
        }
        return users;
    }

    private (string passwordHash, string passwordSalt) HashPassword(string password) //вынести в хелпер, а может лучше в сервис
    {
        //https://code-maze.com/csharp-hashing-salting-passwords-best-practices/
        int keySize = 64;
        int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        var salt = RandomNumberGenerator.GetBytes(keySize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            hashAlgorithm,
            keySize);
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }

    private (string passwordHash, string passwordSalt) HashPassword(string password, byte[] salt) //вынести в хелпер, а может лучше в сервис
    {
        //https://code-maze.com/csharp-hashing-salting-passwords-best-practices/
        int keySize = 64;
        int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            hashAlgorithm,
            keySize);
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }
}
