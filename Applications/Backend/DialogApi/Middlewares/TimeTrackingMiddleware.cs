using SocialNetworkOtus.Shared.Metrics.OpenTelemetry;
using System.Diagnostics;

namespace DialogApi.Middlewares;

public class TimeTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeTrackingMiddleware> _logger;
    private readonly ApiMetrics _metrics;

    public TimeTrackingMiddleware(
        RequestDelegate next,
        ILogger<TimeTrackingMiddleware> logger,
        ApiMetrics metrics)
    {
        _next = next;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _metrics.AddRequest("all");
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation($"Request to {context.Request.Path} took {stopwatch.ElapsedMilliseconds} ms.");
        _metrics.RecordHandlerDuration("all", stopwatch.ElapsedMilliseconds);
    }
}
