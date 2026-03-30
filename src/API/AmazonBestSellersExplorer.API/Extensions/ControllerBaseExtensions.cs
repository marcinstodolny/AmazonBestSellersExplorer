using AmazonBestSellersExplorer.Domain.Base;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Extensions;

internal static class ControllerBaseExtensions
{
    public static ObjectResult ToProblem(this ControllerBase controller, Result result, int statusCode, string title)
    {
        return controller.ToProblemDetailsResult(result.Errors, statusCode, title);
    }

    public static ObjectResult ToProblem<T>(this ControllerBase controller, Result<T> result, int statusCode, string title)
    {
        return controller.ToProblemDetailsResult(result.Errors, statusCode, title);
    }

    private static ObjectResult ToProblemDetailsResult(this ControllerBase controller, IReadOnlyCollection<string> errors, int statusCode, string title)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = string.Join(" ", errors),
            Instance = controller.HttpContext.Request.Path,
            Extensions =
            {
                ["errors"] = errors
            }
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
