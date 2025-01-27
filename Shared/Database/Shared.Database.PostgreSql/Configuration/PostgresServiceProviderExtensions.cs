using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Configuration;

public static class PostgresServiceProviderExtensions
{
    public static void InitPostgresDatabases(this IServiceProvider services,
        ILogger logger = null)
    {
        var userRepository = services.GetRequiredService<UserRepository>();
        userRepository.Init();
        var messageRepository = services.GetRequiredService<IMessageRepository>();
        messageRepository.Init();
    }
}
