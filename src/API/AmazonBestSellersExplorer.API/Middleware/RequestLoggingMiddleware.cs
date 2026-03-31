using System.Diagnostics;
using AmazonBestSellersExplorer.Application.Abstractions.Authentication;

namespace AmazonBestSellersExplorer.API.Middleware;

internal sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    private static readonly PathString ApiPathPrefix = new("/api");

    public async Task InvokeAsync(HttpContext context, ICurrentUserContext currentUserContext)
    {
        if (!context.Request.Path.StartsWithSegments(ApiPathPrefix))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var statusCode = StatusCodes.Status200OK;

        try
        {
            await next(context);
            statusCode = context.Response.StatusCode;
        }
        catch
        {
            statusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "Handled API request {Method} {Path} -> {StatusCode} in {ElapsedMilliseconds} ms. UserId={UserId}, Username={Username}.",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                currentUserContext.UserId,
                currentUserContext.Username);
        }
    }
}
