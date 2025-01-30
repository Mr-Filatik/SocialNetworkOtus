using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkOtus.Shared.Event.Kafka
{
    public interface IKafkaConsumer<KT, EventType>
    {
        public void Init();
    }
}
