using System.Diagnostics;

namespace DialogApi.Middlewares;

public class TimeTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeTrackingMiddleware> _logger;

    public TimeTrackingMiddleware(
        RequestDelegate next,
        ILogger<TimeTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation($"Request to {context.Request.Path} took {stopwatch.ElapsedMilliseconds} ms.");
    }
}
