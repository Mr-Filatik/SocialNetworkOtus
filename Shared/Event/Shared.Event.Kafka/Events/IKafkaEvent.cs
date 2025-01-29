namespace SocialNetworkOtus.Shared.Event.Kafka.Events;

public interface IKafkaEvent<KT>
{
    public KT GetPartitionKey();
}
