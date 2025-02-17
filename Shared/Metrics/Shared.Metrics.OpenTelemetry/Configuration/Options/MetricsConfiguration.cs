using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkOtus.Shared.Metrics.OpenTelemetry.Configuration.Options
{
    public class MetricsConfiguration
    {
        public string ServiceName { get; set; }

        public string Url { get; set; }
    }
}
