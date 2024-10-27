using Microsoft.Extensions.DependencyInjection;

namespace SocialNetworkOtus.Shared.Cache.Redis.Configuration
{
    public static class RedisCacheServiceProviderExtension
    {
        public static void InitRedisCache(this IServiceProvider services, string endpoint)
        {
            var redis = services.GetRequiredService<ICacher>();
            redis.Connect(endpoint);
        }
    }
}
