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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns> Guid a new user. </returns>
    public string Add(UserEntity user)
    {
        user.Id = Guid.NewGuid().ToString();
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        var passData = HashPassword(user.PasswordHash);
        using var command = new NpgsqlCommand(
            $"""
            INSERT INTO users (user_id ,first_name, second_name, password_hash, password_salt, gender, date_of_birth)
            VALUES (@user_id, @first_name, @second_name, @password_hash, @password_salt, @gender, @date_of_birth)
            ON CONFLICT (user_id) DO NOTHING;
            """, connection);
        command.Parameters.AddWithValue("user_id", user.Id);
        command.Parameters.AddWithValue("first_name", user.FirstName);
        command.Parameters.AddWithValue("second_name", user.SecondName);
        command.Parameters.AddWithValue("password_hash", passData.passwordHash);
        command.Parameters.AddWithValue("password_salt", passData.passwordSalt);
        command.Parameters.AddWithValue("gender", (user.Gender == Entities.Types.Gender.Male));
        command.Parameters.AddWithValue("date_of_birth", user.DateOfBirth);
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
            };
        }
        return null;
    }

    public UserEntity? Search(string firstName, string secondName)
    {
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            $"""
            SELECT *
            FROM users
            WHERE first_name=@first_name AND second_name=@second_name;
            """, connection);
        command.Parameters.AddWithValue("first_name", firstName);
        command.Parameters.AddWithValue("second_name", secondName);
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
            };
        }
        return null;
    }

    private (string passwordHash, string passwordSalt) HashPassword(string password)
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

    private (string passwordHash, string passwordSalt) HashPassword(string password, byte[] salt)
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
