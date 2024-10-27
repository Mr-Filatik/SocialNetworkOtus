using SocialNetworkOtus.Shared.Database.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace SocialNetworkOtus.Shared.Cache.Redis;

public interface ICacher
{
    public void Connect(string endpoint);

    //public void SetLastPost(string userId, int postId);

    //public int? GetLastPost(string userId);

    public bool IsPosts(string userId);

    public void SetPosts(string userId, List<PostEntity> posts);

    public IEnumerable<PostEntity> GetPosts(string userId, int limit, int offset = 0);

    public void SetValue<T>(string key, T value, CacheType type = CacheType.Common, TimeSpan? expireTime = null);

    public T? GetValue<T>(string key, CacheType type = CacheType.Common);
}
