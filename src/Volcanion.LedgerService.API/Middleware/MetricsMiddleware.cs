using System.Diagnostics;

namespace Volcanion.LedgerService.API.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var routePattern = (context.GetEndpoint() as Microsoft.AspNetCore.Routing.RouteEndpoint)?.RoutePattern?.RawText ?? context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            
            Metrics.LedgerMetrics.ApiRequestDuration
                .WithLabels(method, routePattern, statusCode.ToString())
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
