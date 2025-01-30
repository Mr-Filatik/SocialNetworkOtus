using Confluent.Kafka;
using SocialNetworkOtus.Shared.Event.Kafka.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkOtus.Shared.Event.Kafka;

public interface IKafkaProducer<KT, EventType>
    where EventType : IKafkaEvent<KT>
{
    public void Init();

    public void Produce(EventType @event);
}
