using Confluent.Kafka;
using SocialNetworkOtus.Shared.Event.Kafka.Events;
using System.Text.Json;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public class JsonSerialiser<KT, EventType> : ISerializer<EventType>, IDeserializer<EventType>
    where EventType : IKafkaEvent<KT>
{
    public EventType Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            return default; // or null
        }

        return (EventType)JsonSerializer.Deserialize(data, typeof(EventType));
    }

    public byte[] Serialize(EventType data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}
