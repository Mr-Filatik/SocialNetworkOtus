//using Confluent.Kafka;
//using Microsoft.Extensions.Logging;
//using SocialNetworkOtus.Shared.Event.Kafka.Events;

//namespace SocialNetworkOtus.Shared.Event.Kafka;

//public class Consumer<EventType>
//    where EventType : IKafkaEvent
//{
//    private IConsumer<string, EventType> _consumer;

//    private readonly ILogger<Consumer<EventType>> _logger;

//    public Consumer(ILogger<Consumer<EventType>> logger)
//    {
//        _logger = _logger ?? throw new ArgumentNullException();
//    }

//    public void Init()
//    {
//        var config = new ConsumerConfig()
//        {
//            BootstrapServers = "",
//        };

//        _consumer = new ConsumerBuilder<string, EventType>(config)
//            .SetValueDeserializer(new JsonSerialiser<EventType>())
//            .SetLogHandler(LogHandler)
//            .SetErrorHandler(ErrorHandler)
//            .SetOffsetsCommittedHandler(OffsetsCommittedHandler)
//            .Build();
//    }

//    private void OffsetsCommittedHandler(IConsumer<string, EventType> consumer, CommittedOffsets offsets)
//    {
//        var producer = new Producer<PostCreatedEvent>(null)
//            .SetTopic("test");
//    }

//    private void LogHandler(IConsumer<string, EventType> consumer, LogMessage log)
//    {
//        switch (log.Level)
//        {
//            case SyslogLevel.Emergency:
//                break;
//            case SyslogLevel.Alert:
//                break;
//            case SyslogLevel.Critical:
//                break;
//            case SyslogLevel.Error:
//                break;
//            case SyslogLevel.Warning:
//                break;
//            case SyslogLevel.Notice:
//                break;
//            case SyslogLevel.Info:
//                break;
//            case SyslogLevel.Debug:
//                break;
//            default:
//                break;
//        }
//    }

//    private void ErrorHandler(IConsumer<string, EventType> consumer, Error error)
//    {
//        if (error.IsError)
//        {
//            _logger.LogError($"consumer error: {error.Reason}");
//        }
//    }
//}
