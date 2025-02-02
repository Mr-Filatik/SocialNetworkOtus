namespace SocialNetworkOtus.Applications.Backend.MainApi.Middlewares;

public class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-ID";

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(HeaderName))
        {
            var requestId = Guid.NewGuid().ToString();
            context.Request.Headers.Add(HeaderName, requestId);
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = context.Request.Headers[HeaderName];
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
