using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Event.Kafka.Events;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public class Producer<KT, EventType> : IKafkaProducer<KT, EventType>
    where EventType : IKafkaEvent<KT>
{
    public string Topic { get; set; } = "post_created";

    private IProducer<KT, EventType> _producer;

    private readonly ILogger<Producer<KT, EventType>> _logger;
    private readonly IConfiguration _configuration;

    public Producer(ILogger<Producer<KT, EventType>> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException();
        _configuration = configuration ?? throw new ArgumentNullException();
    }

    public void Init()
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = _configuration.GetSection("Kafka:BootstrapServers").Value,
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