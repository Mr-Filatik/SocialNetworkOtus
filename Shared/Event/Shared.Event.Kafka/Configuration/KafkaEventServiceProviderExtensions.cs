using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Shared.Event.Kafka.Events;

namespace SocialNetworkOtus.Shared.Event.Kafka.Configuration
{
    public static class KafkaEventServiceProviderExtensions
    {
        public static void InitKafkaEvent(this IServiceProvider services,
        ILogger logger = null)
        {
            var producer = services.GetRequiredService<IKafkaProducer<string, PostCreatedEvent>>();
            producer.Init();

            //var consumer = services.GetRequiredService<IKafkaConsumer<string, PostCreatedEvent>>();
            //consumer.Init();

            var postConsumer = services.GetRequiredService<PostConsumer>();
            postConsumer.Init();
        }
    }
}
