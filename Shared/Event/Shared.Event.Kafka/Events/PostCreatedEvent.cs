namespace SocialNetworkOtus.Shared.Event.Kafka.Events;

public class PostCreatedEvent : IKafkaEvent<string>
{
    public int PostId { get; set; }
    public string AuthorId { get; set; }
    public DateTime CreatedTime { get; set; }
    public string Content { get; set; }

    public string GetPartitionKey()
    {
        return Guid.NewGuid().ToString();
    }
}
