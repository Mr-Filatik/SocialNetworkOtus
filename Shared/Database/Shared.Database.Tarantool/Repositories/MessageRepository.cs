using Microsoft.Extensions.Logging;
using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;
using Shared.Database.Abstract;
using Shared.Database.Tarantool.Configuration.Options;
using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Shared.Database.Tarantool.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly int _limit = 100;
    private Box _client;

    private readonly ILogger<MessageRepository> _logger;
    private readonly TarantoolConfiguration _configuration;

    public MessageRepository(
        ILogger<MessageRepository> logger,
        TarantoolConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public void Init()
    {
        try
        {
            _client = Box.Connect($"{_configuration.Host}:{_configuration.Port}").Result;
            var schema = _client.GetSchema();
            var space = schema["messages"];
        }
        catch (ArgumentException ex)
        {
            throw new Exception($"Space with name 'messages' was not found!");
        }
        catch (AggregateException)
        {
            throw new Exception($"The tarantool instance is not running. Run init.lua file.");
        }
    }

    public void Create(MessageEntity entity)
    {
        entity.SendingTime = DateTime.UtcNow;
        var fromToHash = GetDeterministicHashCode(entity.From, entity.To);

        var turple = TarantoolTuple.Create(entity.From, entity.To, fromToHash, entity.SendingTime.ToString("O"), entity.Text);
        var result = _client.Call<TarantoolTuple<string, string, long, string, string>, TarantoolTuple<long>>("message_create", turple).Result;
        var res = result.Data.FirstOrDefault();
        if (res != null && res.Item1 != -1)
        {
            // ok
        }
        else
        {
            // error
        }
    }

    public IEnumerable<MessageEntity> GetListInRange(string firstUser, string secondUser, long newest, long oldest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        var turple = TarantoolTuple.Create(fromToHash, newest, oldest, _limit);
        var result = _client.Call<TarantoolTuple<long, long, long, int>, TarantoolTuple<long, string, string, long, string, string>[]>("message_get_list_in_range", turple).Result;
        var res = result.Data.FirstOrDefault();
        if (res != null)
        {
            var output = res.Select(x => new MessageEntity()
            {
                Id = x.Item1,
                From = x.Item2,
                To = x.Item3,
                FromToHash = x.Item4,
                SendingTime = DateTime.Parse(x.Item5),
                Text = x.Item6
            });

            return output;
        }
        else
        {
            return new List<MessageEntity>();
        }
    }

    public IEnumerable<MessageEntity> GetListLatest(string firstUser, string secondUser)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        var turple = TarantoolTuple.Create(fromToHash, _limit);
        var result = _client.Call<TarantoolTuple<long, int>, TarantoolTuple<long, string, string, long, string, string>[]>("message_get_list_latest", turple).Result;
        var res = result.Data.FirstOrDefault();
        if (res != null)
        {
            var output = res.Select(x => new MessageEntity()
            {
                Id = x.Item1,
                From = x.Item2,
                To = x.Item3,
                FromToHash = x.Item4,
                SendingTime = DateTime.Parse(x.Item5),
                Text = x.Item6
            });

            return output;
        }
        else
        {
            return new List<MessageEntity>();
        }
    }

    public IEnumerable<MessageEntity> GetListNewest(string firstUser, string secondUser, long newest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        var turple = TarantoolTuple.Create(fromToHash, newest, _limit);
        var result = _client.Call<TarantoolTuple<long, long, int>, TarantoolTuple<long, string, string, long, string, string>[]>("message_get_list_newest", turple).Result;
        var res = result.Data.FirstOrDefault();
        if (res != null)
        {
            var output = res.Select(x => new MessageEntity()
            {
                Id = x.Item1,
                From = x.Item2,
                To = x.Item3,
                FromToHash = x.Item4,
                SendingTime = DateTime.Parse(x.Item5),
                Text = x.Item6
            });

            return output;
        }
        else
        {
            return new List<MessageEntity>();
        }
    }

    public IEnumerable<MessageEntity> GetListOldest(string firstUser, string secondUser, long oldest)
    {
        var fromToHash = GetDeterministicHashCode(firstUser, secondUser);

        var turple = TarantoolTuple.Create(fromToHash, oldest, _limit);
        var result = _client.Call<TarantoolTuple<long, long, int>, TarantoolTuple<long, string, string, long, string, string>[]>("message_get_list_oldest", turple).Result;
        var res = result.Data.FirstOrDefault();
        if (res != null)
        {
            var output = res.Select(x => new MessageEntity()
            {
                Id = x.Item1,
                From = x.Item2,
                To = x.Item3,
                FromToHash = x.Item4,
                SendingTime = DateTime.Parse(x.Item5),
                Text = x.Item6
            });

            return output;
        }
        else
        {
            return new List<MessageEntity>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strs"></param>
    /// <returns></returns>
    /// <remarks> <see href="https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/"/> </remarks>
    private long GetDeterministicHashCode(params string[] strs)
    {
        var sortStrs = strs.Order();
        unchecked
        {
            int hash1 = (5381 << 16) + 5381; //change to long
            int hash2 = hash1; //change to long

            for (int i = 0; i < sortStrs.First().Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ sortStrs.First()[i];
                if (i == sortStrs.First().Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ sortStrs.First()[i + 1];
            }

            for (int i = 0; i < sortStrs.Last().Length; i += 2)
            {
                hash1 = ((hash1 << 10) + hash1) ^ sortStrs.Last()[i];
                if (i == sortStrs.Last().Length - 1)
                    break;
                hash2 = ((hash2 << 10) + hash2) ^ sortStrs.Last()[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}