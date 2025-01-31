using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Database.Abstract;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Configuration;

public static class PostgresServiceCollectionExtensions
{
    public static void AddPostgres(this IServiceCollection services, 
        IConfiguration configuration,
        Action<PostgresOptions> options = null,
        ILogger logger = null)
    {
        var postgreOptions = new PostgresOptions()
        {
            MasterConnectionString = configuration.GetSection("Postgres:ConnectionStrings:Master").Value,
            ReplicaConnectionStrings = configuration.GetSection("Postgres:ConnectionStrings:Replicas").Get<string[]>(),
            ShardConnectionStrings = configuration.GetSection("Postgres:ConnectionStrings:Shards").Get<string[]>(),
        };
        options?.Invoke(postgreOptions);
        services.AddSingleton(postgreOptions);

        services.AddSingleton<PostgreDatabaseSelector>();
        services.AddSingleton<UserRepository>();

        services.AddSingleton<IMessageRepository, MessageRepository>();
        //services.AddSingleton<IMessageRepository, MessageLuaRepository>();
    }
}
