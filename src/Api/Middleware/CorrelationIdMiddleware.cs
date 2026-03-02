using Serilog.Context;

namespace Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue) &&
                            !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : context.TraceIdentifier;

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Correlation ID set to {CorrelationId}", correlationId);
            await _next(context);
        }
    }
}
