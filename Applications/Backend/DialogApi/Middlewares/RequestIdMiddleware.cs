namespace DialogApi.Middlewares;

public class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-ID";
    public const string ServerHeaderName = "Server-Name";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public RequestIdMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
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
            context.Response.Headers[ServerHeaderName] = _configuration["Application:ServerName"];
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
