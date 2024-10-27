namespace SocialNetworkOtus.Shared.Cache.Redis;

public enum CacheType
{
    Common = 0,

    Global = 1,

    /// <summary>
    /// Кеш для хранения списка подписок (друзей).
    /// </summary>
    Friends = 2,

    /// <summary>
    /// Кеш для хранения последних прогруженных постов для пользователей.
    /// </summary>
    UserLastPosts = 4,

    /// <summary>
    /// Кеш для хранения постов.
    /// </summary>
    Posts = 5,
}
