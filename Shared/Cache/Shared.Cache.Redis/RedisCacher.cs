using SocialNetworkOtus.Shared.Database.Entities;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialNetworkOtus.Shared.Cache.Redis;

public class RedisCacher : ICacher
{
    private ConnectionMultiplexer _connectionMultiplexer;
    private TimeSpan _expire = TimeSpan.FromMinutes(5);

    public void Connect(string endpoint)
    {
        ConfigurationOptions options = new ConfigurationOptions()
        {
            EndPoints = { endpoint },
            AbortOnConnectFail = false,
        };
        _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
    }

    public bool IsPosts(string userId)
    {
        var database = _connectionMultiplexer.GetDatabase((int)CacheType.Posts);
        return database.KeyExists(userId);
    }

    public void SetPosts(string userId, List<PostEntity> posts)
    {
        var database = _connectionMultiplexer.GetDatabase((int)CacheType.Posts);
        for (int i = posts.Count() - 1; i >= 0; i--)
        {
            database.ListLeftPushAsync((RedisKey)userId, (RedisValue)JsonSerializer.SerializeToUtf8Bytes(posts[i]));
        }
        database.KeyExpire((RedisKey)userId, _expire);
        database.ListTrim((RedisKey)userId, 0, 1000);
    }

    public IEnumerable<PostEntity> GetPosts(string userId, int limit, int offset = 0)
    {
        var database = _connectionMultiplexer.GetDatabase((int)CacheType.Posts);
        var rValues = database.ListRange(new RedisKey(userId), offset, offset + limit);
        var posts = new List<PostEntity>();
        foreach (var r in rValues)
        {
            posts.Add((PostEntity)JsonSerializer.Deserialize((byte[])r, typeof(PostEntity)));
        }
        database.KeyExpire((RedisKey)userId, _expire);
        return posts;
    }

    //public void SetLastPost(string userId, int postId)
    //{
    //    SetValue<int?>(userId, postId, CacheType.UserLastPosts, _expire);
    //}

    //public int? GetLastPost(string userId)
    //{
    //    return GetValue<int?>(userId, CacheType.UserLastPosts);
    //}

    public void SetValue<T>(string key, T value, CacheType type = CacheType.Common, TimeSpan? expireTime = null)
    {
        var database = _connectionMultiplexer.GetDatabase((int)type);
        var rKey = new RedisKey(key);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var rValue = (RedisValue)bytes;
        database.StringSet(rKey, rValue, expireTime);
    }

    public T? GetValue<T>(string key, CacheType type = CacheType.Common)
    {
        var database = _connectionMultiplexer.GetDatabase((int)type);
        var rValue = database.StringGet(key);
        if (rValue == RedisValue.Null)
        {
            return default;
        }
        var data = (byte[])rValue;
        return (T)JsonSerializer.Deserialize(data, typeof(T));
    }
}
