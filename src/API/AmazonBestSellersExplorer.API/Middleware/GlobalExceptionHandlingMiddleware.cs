using System.Text.Json;
using AmazonBestSellersExplorer.API.Common;

namespace AmazonBestSellersExplorer.API.Middleware;

internal sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger,
    IHostEnvironment hostEnvironment)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}.", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var errors = hostEnvironment.IsDevelopment()
                ? new[] { "An unexpected error occurred while processing the request.", exception.Message }
                : new[] { "An unexpected error occurred while processing the request." };

            var problemDetails = ApiProblemDetailsFactory.Create(
                context,
                errors,
                StatusCodes.Status500InternalServerError,
                ApiProblemTitles.UnhandledException);

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonSerializerOptions));
        }
    }
}
