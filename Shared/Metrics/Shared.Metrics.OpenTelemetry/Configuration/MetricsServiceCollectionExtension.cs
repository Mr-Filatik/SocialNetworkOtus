using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using SocialNetworkOtus.Shared.Metrics.OpenTelemetry.Configuration.Options;

namespace SocialNetworkOtus.Shared.Metrics.OpenTelemetry.Configuration
{
    public static class MetricsServiceCollectionExtension
    {
        public static void AddApiMetrics(this IServiceCollection services, IConfiguration config, Action<MetricsConfiguration> configuration = null, ILogger logger = null)
        {
            var conf = new MetricsConfiguration()
            {
                ServiceName = config["Metrics:ServiceName"],
                Url = config["Metrics:Url"],
            };
            configuration?.Invoke(conf);

            //if (!conf.Url.EndsWith("/"))
            //{
            //    conf.Url += "/";
            //}

            var mymetrics = new ApiMetrics(conf.ServiceName);
            services.AddSingleton(typeof(ApiMetrics), mymetrics);

            //var meterProvider = Sdk.CreateMeterProviderBuilder()
            //    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(conf.ServiceName))
            //    .AddMeter(metrics.Meter.Name)
            //    .AddPrometheusExporter()
            ////.AddPrometheusHttpListener(options =>
            ////{
            ////    options.UriPrefixes = new[] { conf.Url };
            ////})
            //.Build();
            //services.AddSingleton(meterProvider);

            //logger?.LogInformation($"Getting the service metrics is available at {conf.Url + "/"}metrics.");

            var otel = services.AddOpenTelemetry();

            // Configure OpenTelemetry Resources with the application name
            otel.ConfigureResource(resource => resource
                .AddService(serviceName: conf.ServiceName));

            // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
            otel.WithMetrics(metrics => metrics
                // Metrics provider from OpenTelemetry
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(mymetrics.Meter.Name)
                //// Metrics provides by ASP.NET Core in .NET 8
                //.AddMeter("Microsoft.AspNetCore.Hosting")
                //.AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                //// Metrics provided by System.Net libraries
                //.AddMeter("System.Net.Http")
                //.AddMeter("System.Net.NameResolution")
                .AddPrometheusExporter());

        }

        public static void UseApiMetrics(this WebApplication service)
        {
            service.MapPrometheusScrapingEndpoint();
        }
    }
}
