using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Event.Kafka.Events;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public class Producer<KT, EventType> : IKafkaProducer<KT, EventType>
    where EventType : IKafkaEvent<KT>
{
    public string Topic { get; set; } = "post_created";

    private IProducer<KT, EventType> _producer;

    private readonly ILogger<Producer<KT, EventType>> _logger;

    public Producer(ILogger<Producer<KT, EventType>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException();
    }

    public void Init()
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:9092",
        };

        _producer = new ProducerBuilder<KT, EventType>(config)
            .SetValueSerializer(new JsonSerialiser<KT, EventType>())
            .SetLogHandler(LogHandler)
            .SetErrorHandler(ErrorHandler)
            .Build();
    }

    public void Produce(EventType @event)
    {
        var message = new Message<KT, EventType>()
        {
            Key = @event.GetPartitionKey(),
            Value = @event,
        };

        _producer.Produce(Topic, message, DeliveryHandler);
    }

    private void DeliveryHandler(DeliveryReport<KT, EventType> report)
    {
        switch (report.Status)
        {
            case PersistenceStatus.NotPersisted:
                break;
            case PersistenceStatus.PossiblyPersisted:
                break;
            case PersistenceStatus.Persisted:
                break;
            default:
                break;
        }
    }

    private void LogHandler(IProducer<KT, EventType> producer, LogMessage log)
    {
        switch (log.Level)
        {
            case SyslogLevel.Emergency:
                break;
            case SyslogLevel.Alert:
                break;
            case SyslogLevel.Critical:
                break;
            case SyslogLevel.Error:
                break;
            case SyslogLevel.Warning:
                break;
            case SyslogLevel.Notice:
                break;
            case SyslogLevel.Info:
                break;
            case SyslogLevel.Debug:
                break;
            default:
                break;
        }
    }

    private void ErrorHandler(IProducer<KT, EventType> producer, Error error)
    {
        if (error.IsError)
        {
            _logger.LogError($"Producer error: {error.Reason}");
        }
    }
}

public static class ProducerExtensions
{
    public static Producer<KT, EventType> SetTopic<KT, EventType>(this Producer<KT, EventType> producer, string topic)
        where EventType : IKafkaEvent<KT>
    {
        producer.Topic = topic;
        return producer;
    }
}