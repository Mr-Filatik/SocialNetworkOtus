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
        }
        return _dataSource;
    }
}
