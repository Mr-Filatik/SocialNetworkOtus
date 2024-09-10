using Npgsql;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;

namespace SocialNetworkOtus.Shared.Database.PostgreSql;

public class PostgreDatabaseSelector
{
    private NpgsqlDataSource _dataSource;

    private readonly PostgreOptions _options;

    public PostgreDatabaseSelector(PostgreOptions options)
    {
        _options = options;
    }

    public NpgsqlDataSource GetDatabase()
    {
        if (_dataSource is null)
        {
            _dataSource = NpgsqlDataSource.Create(_options.ConnectionString);

            try
            {
                using var connection = _dataSource.OpenConnection();
                using var command = new NpgsqlCommand(
                    """
                    CREATE TABLE IF NOT EXISTS users (
                        user_id text primary key,
                        first_name text NOT NULL,
                        second_name text NOT NULL,
                        password_hash text NOT NULL,
                        gender boolean NOT NULL,
                        date_of_birth timestamp NOT NULL);
                    """, connection);
                using var reader = command.ExecuteReader();

                using var connection1 = _dataSource.OpenConnection();
                using var ins1command = new NpgsqlCommand(
                    """
                    INSERT INTO users (user_id ,first_name, second_name, password_hash, gender, date_of_birth)
                    VALUES ('001', 'FirstName001', 'SecondName001', '001', true, '2001-01-01 00:00:00')
                    ON CONFLICT (user_id) DO NOTHING;
                    """, connection1);
                using var ins1reader = ins1command.ExecuteReader();

                using var connection2 = _dataSource.OpenConnection();
                using var ins2command = new NpgsqlCommand(
                    """
                    INSERT INTO users (user_id ,first_name, second_name, password_hash, gender, date_of_birth)
                    VALUES ('002', 'FirstName002', 'SecondName002', '002', false, '2002-02-02 00:00:00')
                    ON CONFLICT (user_id) DO NOTHING;
                    """, connection2);
                using var ins2reader = ins2command.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error {e.Message}");
            }
        }
        return _dataSource;
    }
}
