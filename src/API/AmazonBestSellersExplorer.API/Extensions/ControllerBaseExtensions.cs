using AmazonBestSellersExplorer.Domain.Base;
using AmazonBestSellersExplorer.API.Common;
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
        var problemDetails = ApiProblemDetailsFactory.Create(controller.HttpContext, errors, statusCode, title);

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
