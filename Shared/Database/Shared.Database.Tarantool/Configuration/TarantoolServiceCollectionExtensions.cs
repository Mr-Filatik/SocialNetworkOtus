using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Database.Abstract;
using Shared.Database.Tarantool.Configuration.Options;
using SocialNetworkOtus.Shared.Database.Tarantool.Repositories;

namespace Shared.Database.Tarantool.Configuration;

public static class TarantoolServiceCollectionExtensions
{
    public static void AddTarantool(this IServiceCollection services,
        IConfiguration configuration,
        Action<TarantoolConfiguration>? options = null,
        ILogger? logger = null)
    {
        var tarantoolConfig = new TarantoolConfiguration()
        {
            Host = configuration.GetSection("Tarantool:Host").Value,
            Port = int.Parse(configuration.GetSection("Tarantool:Port").Value),
            User = configuration.GetSection("Tarantool:User").Value,
            Password = configuration.GetSection("Tarantool:Password").Value,
        };
        options?.Invoke(tarantoolConfig);
        services.AddSingleton(tarantoolConfig);
        logger?.LogInformation($"Resister TarantoolConfiguration. Type: Singleton.");

        services.AddSingleton<IMessageRepository, MessageRepository>();
        logger?.LogInformation($"Resister MessageRepository as IMessageRepository. Type: Singleton.");
    }
}
