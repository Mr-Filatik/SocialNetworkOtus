using Microsoft.Extensions.DependencyInjection;
using SocialNetworkOtus.Shared.Event.Kafka.Events;

namespace SocialNetworkOtus.Shared.Event.Kafka.Configuration
{
    public static class KafkaEventServiceCollectionExtensions
    {
        public static void AddKafkaEvent(this IServiceCollection services)
        {
            services.AddSingleton<IKafkaProducer<string, PostCreatedEvent>, Producer<string, PostCreatedEvent>>();

            //services.AddSingleton<IKafkaConsumer<string, PostCreatedEvent>, PostConsumer>();
            services.AddSingleton<PostConsumer>();
        }
    }
}
