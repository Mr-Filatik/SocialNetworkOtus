﻿using System.Net.WebSockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using SocialNetworkOtus.Shared.Database.Entities;
using System;

namespace SocialNetworkOtus
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var clientId = "11111111-1111-1111-1111-111111111111";
            Console.WriteLine($"Hello, {clientId}!");

            Console.ReadKey();

            var client = new ClientWebSocket();

            Console.WriteLine("Connecting...");
            //await client.ConnectAsync(new Uri($"ws://localhost:5193/ws?user={clientId}"), CancellationToken.None);
            await client.ConnectAsync(new Uri($"ws://localhost:5253/api/post/feed/posted?user={clientId}"), CancellationToken.None); //7180 wss 
            Console.WriteLine("Connected!");

            var receiveTask = Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];
                while (true)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    //var messageStr = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var buf = new byte[result.Count];
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = buffer[i];
                    }
                    var message = (PostEntity)JsonSerializer.Deserialize(buf, typeof(PostEntity));
                    if (message != null)
                    {
                        Console.WriteLine($"Add new message from {message.AuthorId}: {message.Content}");
                    }
                }
            });

            await receiveTask;
        }
    }
}
