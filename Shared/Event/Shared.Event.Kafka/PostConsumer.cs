using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Database.Entities;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using SocialNetworkOtus.Shared.Event.Kafka.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialNetworkOtus.Shared.Event.Kafka
{
    public interface IPostConsumer
    {
        public void CreateConnection(string name, WebSocket socket);
        public Task SendData(string name, PostEntity entity);
        public Task SendData(IEnumerable<string> names, PostEntity entity);
        public void CloseConnection(string name);
    }

    public class PostConsumer : BaseConsumer<string, PostCreatedEvent>, IPostConsumer
    {
        private readonly UserRepository _userRepository;

        public PostConsumer(ILogger<PostConsumer> logger, IConfiguration configuration, UserRepository userRepository) : base(logger, configuration)
        {
            _userRepository = userRepository;
        }

        public override void Consume(ConsumeResult<string, PostCreatedEvent>? result)
        {
            _logger.LogInformation($"Read data: {result.Message.Value.Content}.");
            var friends = _userRepository.GetFriendIds(result.Message.Value.AuthorId);

            foreach (var friend in friends)
            {
                _logger.LogInformation($"Friends ({result.Message.Value.AuthorId}): {friend}.");
            }

            SendData(friends, new PostEntity()
            {
                PostId = 0,
                Content = result.Message.Value.Content,
                AuthorId = result.Message.Value.AuthorId,
            }).Wait();
        }

        private readonly Dictionary<string, WebSocket> _webSockets = new Dictionary<string, WebSocket>();

        public void CreateConnection(string name, WebSocket socket)
        {
            if (!_webSockets.ContainsKey(name))
            {
                _webSockets.Add(name, socket);
                _logger.LogInformation($"Create socket for name {name}");
            }
        }

        public async Task SendData(string name, PostEntity entity)
        {
            if (_webSockets.TryGetValue(name, out WebSocket socket))
            {
                //var message = $"Current time is: {DateTime.Now.ToString("O")} for user {name}";
                var bytes = JsonSerializer.SerializeToUtf8Bytes(entity);
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                if (socket.State == WebSocketState.Open)
                {
                    _logger.LogInformation($"Send data for socket for name {name}");
                    await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task SendData(IEnumerable<string> names, PostEntity entity)
        {
            foreach (string name in names)
            {
                await SendData(name, entity);
            }
        }

        public void CloseConnection(string name)
        {
            if (_webSockets.ContainsKey(name))
            {
                _webSockets.Remove(name);
                _logger.LogInformation($"Close socket for name {name}");
            }
        }
    }
}
