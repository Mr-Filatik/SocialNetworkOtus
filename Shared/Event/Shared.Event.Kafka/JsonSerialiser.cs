using Confluent.Kafka;
using SocialNetworkOtus.Shared.Event.Kafka.Events;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public class JsonSerialiser<KT, EventType> : ISerializer<EventType>, IDeserializer<EventType>
    where EventType : IKafkaEvent<KT>
{
    public EventType Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize(EventType data, SerializationContext context)
    {
        throw new NotImplementedException();
    }
}
