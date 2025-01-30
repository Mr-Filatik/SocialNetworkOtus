using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Event.Kafka.Events;
using System.Text.RegularExpressions;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public abstract class BaseConsumer<KT, EventType> : IKafkaConsumer<KT, EventType>
    where EventType : IKafkaEvent<KT>
{
    public string Topic { get; set; } = "post_created";

    protected IConsumer<KT, EventType> _consumer;
    protected CancellationTokenSource _tokenSource;

    protected readonly ILogger<BaseConsumer<KT, EventType>> _logger;

    public BaseConsumer(ILogger<BaseConsumer<KT, EventType>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException();
    }

    public abstract void Consume(ConsumeResult<KT, EventType>? result);

    public void Init()
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9092",
            GroupId = "main-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _consumer = new ConsumerBuilder<KT, EventType>(config)
            .SetValueDeserializer(new JsonSerialiser<KT, EventType>())
            .SetLogHandler(LogHandler)
            .SetErrorHandler(ErrorHandler)
            .SetOffsetsCommittedHandler(OffsetsCommittedHandler)
            .Build();

        _tokenSource = new CancellationTokenSource();
        var consumerToken = _tokenSource.Token;

        var task = Task.Run(() =>
        {
            _consumer.Subscribe(Topic);
            _logger.LogInformation($"KafkaEventConsumer start subscribe for topic {Topic}.");

            while (!consumerToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(consumerToken);

                    // Обработка сообщения
                    //
                    _logger.LogInformation($"Consume {result.Message.Key}.");
                    Consume(result);
                    //
                }
                catch (OperationCanceledException ex)
                {
                    //это блок необязательный, т.к. он просто значит завершение считывания
                    //_logger.LogInformation(ex, $"End subscribe for topic {channel}.");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, $"KafkaEventConsumer error consuming message.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"KafkaEventConsumer unexpected error.");
                }
            }

            _logger.LogInformation($"KafkaEventConsumer end subscribe for topic {Topic}.");

            _consumer?.Unsubscribe();
            _consumer?.Close();
            _consumer?.Dispose();
            _consumer = null;
        });
    }

    private void OffsetsCommittedHandler(IConsumer<KT, EventType> consumer, CommittedOffsets offsets)
    {
        
    }

    private void LogHandler(IConsumer<KT, EventType> consumer, LogMessage log)
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

    private void ErrorHandler(IConsumer<KT, EventType> consumer, Error error)
    {
        if (error.IsError)
        {
            _logger.LogError($"consumer error: {error.Reason}");
        }
    }
}
