using AmazonBestSellersExplorer.API.Common;
using AmazonBestSellersExplorer.API.Contracts.Auth;
using AmazonBestSellersExplorer.API.Extensions;
using AmazonBestSellersExplorer.Application.Features.Auth.Contracts;
using AmazonBestSellersExplorer.Application.Features.Auth.LoginUser;
using AmazonBestSellersExplorer.Application.Features.Auth.RegisterUser;
using AmazonBestSellersExplorer.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonBestSellersExplorer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.Username, request.Password),
            cancellationToken);

        return result.IsFailed ? this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.ValidationFailed) : Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginUserCommand(request.Username, request.Password),
            cancellationToken);

        if (result.TryGetValue(out var response))
        {
            return Ok(response);
        }

        if (result.Errors.Count == 1
            && string.Equals(result.FirstError, ApplicationMessages.Auth.InvalidCredentials, StringComparison.Ordinal))
        {
            return this.ToProblem(result, StatusCodes.Status401Unauthorized, ApiProblemTitles.AuthenticationFailed);
        }

        return result.IsFailed
            ? this.ToProblem(result, StatusCodes.Status400BadRequest, ApiProblemTitles.ValidationFailed)
            : Ok(result.Value);
    }
}
