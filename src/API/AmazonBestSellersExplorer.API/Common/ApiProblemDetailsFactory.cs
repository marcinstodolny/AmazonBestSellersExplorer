using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Common;

internal static class ApiProblemDetailsFactory
{
    public static ProblemDetails Create(
        HttpContext httpContext,
        IReadOnlyCollection<string> errors,
        int statusCode,
        string title,
        string? detail = null)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail ?? string.Join(" ", errors),
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["errors"] = errors,
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
    }
}
