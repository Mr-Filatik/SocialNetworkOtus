using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.Entities;
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
            using var testconnection = _databaseSelector.GetDatabase().OpenConnection();
            using var testcommand = new NpgsqlCommand(
                """
                    DROP TABLE IF EXISTS users;
                    """, testconnection);
            using var testreader = testcommand.ExecuteReader();

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
                        date_of_birth timestamp NOT NULL,
                        city text NOT NULL,
                        interests text[] NOT NULL);
                    """, connection);
            using var reader = command.ExecuteReader();

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
            INSERT INTO users (user_id ,first_name, second_name, password_hash, password_salt, gender, date_of_birth, city, interests)
            VALUES (@user_id, @first_name, @second_name, @password_hash, @password_salt, @gender, @date_of_birth, @city, @interests)
            ON CONFLICT (user_id) DO NOTHING;
            """, connection);
        command.Parameters.AddWithValue("user_id", user.Id);
        command.Parameters.AddWithValue("first_name", user.FirstName);
        command.Parameters.AddWithValue("second_name", user.SecondName);
        command.Parameters.AddWithValue("password_hash", passData.passwordHash);
        command.Parameters.AddWithValue("password_salt", passData.passwordSalt);
        command.Parameters.AddWithValue("gender", (user.Gender == Entities.Types.Gender.Male));
        command.Parameters.AddWithValue("date_of_birth", user.DateOfBirth.Date);
        command.Parameters.AddWithValue("city", user.City);
        command.Parameters.AddWithValue("interests", user.Interests);
        using var reader = command.ExecuteReader();

        return user.Id;
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
                DateOfBirth = DateTime.Parse(reader["date_of_birth"].ToString()),
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
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
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
                    Id = reader["user_id"].ToString(),
                    FirstName = reader["first_name"].ToString(),
                    SecondName = reader["second_name"].ToString(),
                    Gender = bool.Parse(reader["gender"].ToString()) ? Entities.Types.Gender.Male : Entities.Types.Gender.Female,
                    DateOfBirth = DateTime.Parse(reader["date_of_birth"].ToString()),
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
