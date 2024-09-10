using Microsoft.Extensions.Logging;
using Npgsql;
using SocialNetworkOtus.Shared.Database.Entities;
using System.Data.Common;

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

    public void AddUser(User user)
    {
        throw new NotImplementedException();
    }

    public bool CheckUserPassword(string id, string password)
    {
        //var user = _users.FirstOrDefault(x => x.Id == id);
        //if (user is not null)
        //{
        //    var passwordHash = password;
        //    if (user.Password == passwordHash)
        //    {
        //        return true;
        //    }
        //}
        return false;
    }

    //public User GetUserByFirstAndSecondName(string firstName, string secondName)
    //{
    //    return _users.FirstOrDefault(x => x.FirstName == firstName && x.SecondName == secondName);
    //}

    //public User GetUserById(string id)
    //{
    //    using var connection = await _dataSource.OpenConnectionAsync();
    //    return _users.FirstOrDefault(x => x.Id == id);
    //}

    public bool HasUser(string id)
    {
        User user = null;
        using var connection = _databaseSelector.GetDatabase().OpenConnection();
        using var command = new NpgsqlCommand(
            """
            SELECT user_id, first_name, second_name, password_hash, gender, date_of_birth 
            FROM public.users
            WHERE user_id='@insert_id';
            """, connection);
        command.Parameters.AddWithValue("insert_id", id);
        using var reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            _logger.LogInformation("Read");
            reader.Read();
            user = new User()
            {
                //reader.GetFieldValue()
                Id = reader["user_id"].ToString(),
            };
        }
        //var user = _users.FirstOrDefault(x => x.Id == id);
        return user != null;
    }
}
