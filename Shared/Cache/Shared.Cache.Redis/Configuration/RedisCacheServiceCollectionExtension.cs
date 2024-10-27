using Microsoft.Extensions.DependencyInjection;

namespace SocialNetworkOtus.Shared.Cache.Redis.Configuration
{
    public static class RedisCacheServiceCollectionExtension
    {
        public static void AddRedisCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacher, RedisCacher>();
        }
    }
}
